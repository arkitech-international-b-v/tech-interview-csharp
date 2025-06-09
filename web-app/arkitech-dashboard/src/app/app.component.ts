import { Component, Injectable } from '@angular/core';
import { RouterOutlet } from '@angular/router';

// Angular Material Imports
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { HttpClient } from '@angular/common/http';
import { AsyncPipe, JsonPipe } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatGridListModule,
    AsyncPipe,
    JsonPipe,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

@Injectable({ providedIn: 'root' })
export class AppComponent {
  title = 'arkitech-dashboard';
  data$!: Observable<any>;
  constructor(private http: HttpClient) { }

  dataGetAll() {
    // Get the latest data from the server for the crew quarters
    let options = { params: { limit: '1', topic: 'arkitech/ships/vessel1/crew_quarters' } };
    this.data$ = this.http.get('http://localhost:8000/data/all', options);
  }

  ngOnInit() {
    this.dataGetAll();
  }

}
