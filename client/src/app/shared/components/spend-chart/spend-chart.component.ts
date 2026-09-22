import { Component, computed, input } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { CategoryBreakdown } from '../../../core/models/models';

@Component({
  selector: 'app-spend-chart',
  imports: [CurrencyPipe, DecimalPipe],
  templateUrl: './spend-chart.component.html',
  styleUrl: './spend-chart.component.scss'
})
export class SpendChartComponent {
  readonly data = input.required<CategoryBreakdown[]>();

  readonly maxTotal = computed(() => Math.max(...this.data().map((d) => d.total), 1));
  readonly grandTotal = computed(() => this.data().reduce((sum, d) => sum + d.total, 0));

  barWidth(total: number): string {
    return `${(total / this.maxTotal()) * 100}%`;
  }

  share(total: number): number {
    const grand = this.grandTotal();
    return grand === 0 ? 0 : (total / grand) * 100;
  }
}
