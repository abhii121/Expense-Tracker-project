import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummary } from '../../core/models/models';
import { SpendChartComponent } from '../../shared/components/spend-chart/spend-chart.component';
import { TrendChartComponent } from '../../shared/components/trend-chart/trend-chart.component';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, DatePipe, RouterLink, SpendChartComponent, TrendChartComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  readonly summary = signal<DashboardSummary | null>(null);
  readonly loading = signal(true);
  readonly monthLabel = new Date().toLocaleString('default', { month: 'long', year: 'numeric' });

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    const now = new Date();
    this.dashboardService.getSummary(now.getFullYear(), now.getMonth() + 1).subscribe((summary) => {
      this.summary.set(summary);
      this.loading.set(false);
    });
  }
}
