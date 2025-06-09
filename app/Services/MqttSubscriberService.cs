using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HiveMQtt.Client;                    // HiveMQtt core client :contentReference[oaicite:6]{index=6}
using HiveMQtt.Client.Events;             // OnMessageReceivedEventArgs :contentReference[oaicite:7]{index=7}
using HiveMQtt.Client.Options;            // SubscribeOptionsBuilder, HiveMQClientOptionsBuilder 
using HiveMQtt.MQTT5.Types;               // TopicFilter, QualityOfService :contentReference[oaicite:9]{index=9}
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ArkitechDataApi.Models;
using ArkitechDataApi.Services;
using MongoDB.Bson;

namespace ArkitechDataApi.Services
{
    /// <summary>
    /// BackgroundService that connects to an MQTT broker using HiveMQtt,
    /// subscribes to a topic filter, and pushes each incoming message into MongoDB via IDataService.
    /// </summary>
    public class MqttSubscriberService : BackgroundService
    {
        private readonly ILogger<MqttSubscriberService> _logger;
        private readonly IDataService _dataService;
        private readonly MqttSettings _mqttSettings;
        private HiveMQClient? _mqttClient;

        public MqttSubscriberService(
            ILogger<MqttSubscriberService> logger,
            IDataService dataService,
            IOptions<MqttSettings> mqttOptions
        )
        {
            _logger = logger;
            _dataService = dataService;
            _mqttSettings = mqttOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting MQTT subscriber using HiveMQtt...");

            // 1. Build HiveMQtt client options (host, port, optional credentials) :contentReference[oaicite:10]{index=10}
            var clientOptions = new HiveMQClientOptionsBuilder()
                .WithBroker(_mqttSettings.BrokerHost)   // e.g., "broker.hivemq.com" :contentReference[oaicite:11]{index=11}
                .WithPort(_mqttSettings.BrokerPort)     // e.g., 1883 
                // If the broker requires user/password, uncomment:
                // .WithUserName(_mqttSettings.Username!)
                // .WithPassword(_mqttSettings.Password!)
                .Build();

            // 2. Instantiate the client
            _mqttClient = new HiveMQClient(clientOptions);

            // 3. Initial connect (default 5000 ms CONNECT/CONNACK timeout internally) :contentReference[oaicite:13]{index=13}
            try
            {
                _logger.LogInformation(
                    "Connecting to HiveMQ broker at {Host}:{Port}",
                    _mqttSettings.BrokerHost,
                    _mqttSettings.BrokerPort
                );
                await _mqttClient.ConnectAsync();  // uses built-in 5 s timeout :contentReference[oaicite:14]{index=14}
                _logger.LogInformation("Connected to broker successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to MQTT broker within 5 seconds.");
                return; // Exit if initial connection fails
            }

            // 4. Define the per‐message callback handler :contentReference[oaicite:15]{index=15}
            EventHandler<OnMessageReceivedEventArgs> messageHandler = async (_, args) =>
            {
                try
                {
                    string topic = args.PublishMessage.Topic;
                    string payloadString = args.PublishMessage.PayloadAsString;

                    _logger.LogInformation(
                        "Received MQTT message on topic {Topic}: {Payload}",
                        topic,
                        payloadString
                    );

                    // Parse or wrap payload as JSON → BsonDocument
                    BsonDocument payloadBson;
                    try
                    {
                        payloadBson = BsonDocument.Parse(payloadString);
                    }
                    catch
                    {
                        payloadBson = new BsonDocument("raw", payloadString);
                    }

                    // Determine timestamp: if payload has a "timestamp" field, parse it; otherwise UtcNow
                    DateTime timestampUtc = DateTime.UtcNow;
                    if (payloadBson.TryGetValue("timestamp", out var tsToken)
                        && tsToken.IsString
                        && DateTime.TryParse(tsToken.AsString, out var parsedTs))
                    {
                        timestampUtc = parsedTs.ToUniversalTime();
                    }

                    // Build the DataItem model (matching your Mongo document) :contentReference[oaicite:16]{index=16}
                    var dataItem = new DataItem
                    {
                        Topic     = topic,
                        Payload   = payloadBson,
                        Timestamp = timestampUtc
                    };

                    // Insert into MongoDB via IDataService
                    await _dataService.InsertItemAsync(dataItem);
                    _logger.LogInformation(
                        "Inserted document into MongoDB for topic {Topic}",
                        topic
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing incoming MQTT message.");
                }
            };

            // 5. Build SubscribeOptions with a single TopicFilter + handler 
            SubscribeOptions subscribeOptions;
            try
            {
                _logger.LogInformation(
                    "Subscribing to topic filter: {Filter}",
                    _mqttSettings.TopicFilter
                );

                subscribeOptions = new SubscribeOptionsBuilder()
                    .WithSubscription(
                        new TopicFilter(
                            _mqttSettings.TopicFilter,
                            QualityOfService.AtLeastOnceDelivery
                        ),
                        messageHandler
                    )
                    .Build();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to build SubscribeOptions for {Filter}", _mqttSettings.TopicFilter);
                return;
            }

            // 6. Execute the subscribe 
            try
            {
                await _mqttClient.SubscribeAsync(subscribeOptions);
                _logger.LogInformation(
                    "Subscribed to {Filter} with QoS=AtLeastOnceDelivery",
                    _mqttSettings.TopicFilter
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to subscribe to topic filter: {Filter}", _mqttSettings.TopicFilter);
                return; // Exit if subscription fails
            }

            // 7. Keep alive & handle reconnection
            while (!stoppingToken.IsCancellationRequested)
            {
                // Fix: use IsConnected as a property (no parentheses) :contentReference[oaicite:19]{index=19}
                if (!_mqttClient.IsConnected())
                {
                    _logger.LogWarning("MQTT client disconnected. Attempting reconnect...");
                    try
                    {
                        await _mqttClient.ConnectAsync();       // uses default 5 s timeout :contentReference[oaicite:20]{index=20}
                        await _mqttClient.SubscribeAsync(subscribeOptions);
                        _logger.LogInformation("Reconnected and re-subscribed successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Reconnection attempt failed – no CONNACK within 5 s. Retrying in 5 s...");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }

            // 8. On shutdown, disconnect gracefully if still connected :contentReference[oaicite:21]{index=21}
            if (_mqttClient.IsConnected())
            {
                await _mqttClient.DisconnectAsync();
                _logger.LogInformation("Disconnected from MQTT broker.");
            }
        }
    }
}
