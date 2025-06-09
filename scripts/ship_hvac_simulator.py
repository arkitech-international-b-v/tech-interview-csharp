#!/usr/bin/env python3
"""
Ship HVAC Simulator - Publishes HVAC data to HiveMQ MQTT broker
"""
import json
import time
import random
from datetime import datetime
import paho.mqtt.client as mqtt

# Define helper function for units
def get_unit(sensor_name):
    """Return the appropriate unit for a sensor type"""
    units = {
        "temperature": "°C",
        "humidity": "%",
        "co2": "ppm",
        "pressure": "hPa"
    }
    return units.get(sensor_name, "")

# HiveMQ broker configuration
BROKER = "broker.hivemq.com"
PORT = 1883
CLIENT_ID = f"ship-hvac-simulator-{random.randint(0, 1000)}"
TOPIC_PREFIX = "arkitech/ships/vessel1"

# HVAC parameters to simulate
hvac_zones = {
    "bridge": {
        "temperature": {"min": 19.0, "max": 24.0, "current": 21.5},
        "humidity": {"min": 40.0, "max": 65.0, "current": 55.0},
        "co2": {"min": 350.0, "max": 1200.0, "current": 700.0},
        "pressure": {"min": 1000.0, "max": 1020.0, "current": 1013.0},
    },
    "engine_room": {
        "temperature": {"min": 25.0, "max": 32.0, "current": 28.0},
        "humidity": {"min": 50.0, "max": 75.0, "current": 65.0},
        "co2": {"min": 400.0, "max": 1500.0, "current": 900.0},
        "pressure": {"min": 1000.0, "max": 1020.0, "current": 1013.0},
    },
    "crew_quarters": {
        "temperature": {"min": 20.0, "max": 25.0, "current": 22.5},
        "humidity": {"min": 40.0, "max": 60.0, "current": 50.0},
        "co2": {"min": 350.0, "max": 1000.0, "current": 600.0},
        "pressure": {"min": 1000.0, "max": 1020.0, "current": 1013.0},
    }
}

# Connect callback
def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print(f"Connected to HiveMQ broker at {BROKER}:{PORT}")
    else:
        print(f"Failed to connect to broker, return code: {rc}")

# Create MQTT client with Paho MQTT 2.0+ syntax
client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION1)
client.client_id = CLIENT_ID
client.on_connect = on_connect

# Connect to HiveMQ broker
print(f"Connecting to HiveMQ broker at {BROKER}:{PORT}...")
client.connect(BROKER, PORT, 60)
client.loop_start()

try:
    while True:
        for zone, sensors in hvac_zones.items():
            # Update each sensor value with some random drift
            for sensor_name, sensor_data in sensors.items():
                # Random drift within limits
                drift = random.uniform(-0.5, 0.5)
                sensor_data["current"] += drift
                
                # Ensure values stay within min/max range
                sensor_data["current"] = max(sensor_data["min"], min(sensor_data["max"], sensor_data["current"]))
                
                # Create message payload
                payload = {
                    "timestamp": datetime.now().isoformat(),
                    "value": round(sensor_data["current"], 2),
                    "unit": get_unit(sensor_name)
                }
                
                # Publish to MQTT
                topic = f"{TOPIC_PREFIX}/{zone}/{sensor_name}"
                client.publish(topic, json.dumps(payload))
                print(f"Published to {topic}: {payload}")
        
        # Wait before next update
        time.sleep(5)
        
except KeyboardInterrupt:
    print("Simulation stopped")
    client.loop_stop()
    client.disconnect()