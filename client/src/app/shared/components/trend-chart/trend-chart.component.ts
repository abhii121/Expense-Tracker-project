import { AfterViewInit, Component, ElementRef, OnDestroy, ViewChild, effect, input } from '@angular/core';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { MonthlyTrendPoint } from '../../../core/models/models';

Chart.register(...registerables);

const MONTH_LABELS = [
  'Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'
];

@Component({
  selector: 'app-trend-chart',
  imports: [],
  templateUrl: './trend-chart.component.html',
  styleUrl: './trend-chart.component.scss'
})
export class TrendChartComponent implements AfterViewInit, OnDestroy {
  readonly data = input.required<MonthlyTrendPoint[]>();

  @ViewChild('canvas') private canvasRef!: ElementRef<HTMLCanvasElement>;
  private chart: Chart | null = null;
  private viewReady = false;

  constructor() {
    effect(() => {
      const points = this.data();
      if (this.viewReady) {
        this.render(points);
      }
    });
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    this.render(this.data());
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private render(points: MonthlyTrendPoint[]): void {
    const styles = getComputedStyle(document.documentElement);
    const income = styles.getPropertyValue('--series-1').trim() || '#2a78d6';
    const expense = styles.getPropertyValue('--series-2').trim() || '#eb6834';
    const gridline = styles.getPropertyValue('--gridline').trim() || '#e1e0d9';
    const muted = styles.getPropertyValue('--text-muted').trim() || '#898781';

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels: points.map((p) => MONTH_LABELS[p.month - 1]),
        datasets: [
          {
            label: 'Income',
            data: points.map((p) => p.income),
            backgroundColor: income,
            borderRadius: 4,
            maxBarThickness: 24
          },
          {
            label: 'Expense',
            data: points.map((p) => p.expense),
            backgroundColor: expense,
            borderRadius: 4,
            maxBarThickness: 24
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { position: 'top', align: 'end', labels: { color: muted, boxWidth: 10, usePointStyle: true } },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          x: { grid: { display: false }, ticks: { color: muted } },
          y: {
            beginAtZero: true,
            grid: { color: gridline },
            ticks: { color: muted }
          }
        }
      }
    };

    if (this.chart) {
      this.chart.data = config.data;
      this.chart.update();
    } else {
      this.chart = new Chart(this.canvasRef.nativeElement, config);
    }
  }
}
