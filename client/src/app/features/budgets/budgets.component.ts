import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { BudgetService } from '../../core/services/budget.service';
import { CategoryService } from '../../core/services/category.service';
import { Budget, Category } from '../../core/models/models';

@Component({
  selector: 'app-budgets',
  imports: [CurrencyPipe, DecimalPipe, ReactiveFormsModule],
  templateUrl: './budgets.component.html',
  styleUrl: './budgets.component.scss'
})
export class BudgetsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly budgetService = inject(BudgetService);
  private readonly categoryService = inject(CategoryService);

  readonly budgets = signal<Budget[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly showForm = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private readonly now = new Date();
  readonly year = this.now.getFullYear();
  readonly month = this.now.getMonth() + 1;
  readonly monthLabel = this.now.toLocaleString('default', { month: 'long', year: 'numeric' });

  readonly form = this.fb.group({
    categoryId: [0, [Validators.required, Validators.min(1)]],
    monthlyLimit: [0, [Validators.required, Validators.min(1)]]
  });

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((categories) => {
      this.categories.set(categories.filter((c) => c.type === 'Expense'));
    });
    this.load();
  }

  get expenseCategories(): Category[] {
    return this.categories();
  }

  load(): void {
    this.loading.set(true);
    this.budgetService.getForMonth(this.year, this.month).subscribe((budgets) => {
      this.budgets.set(budgets);
      this.loading.set(false);
    });
  }

  startCreate(): void {
    this.errorMessage.set(null);
    const firstAvailable = this.expenseCategories.find(
      (c) => !this.budgets().some((b) => b.categoryId === c.id)
    );
    this.form.reset({ categoryId: firstAvailable?.id ?? 0, monthlyLimit: 0 });
    this.showForm.set(true);
  }

  cancel(): void {
    this.showForm.set(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { categoryId, monthlyLimit } = this.form.getRawValue();
    this.budgetService.upsert({ categoryId: categoryId!, monthlyLimit: monthlyLimit!, year: this.year, month: this.month }).subscribe({
      next: () => {
        this.showForm.set(false);
        this.load();
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Could not save budget.')
    });
  }

  remove(budget: Budget): void {
    if (!confirm(`Delete the budget for ${budget.categoryName}?`)) return;
    this.budgetService.delete(budget.id).subscribe(() => this.load());
  }

  progressPercent(budget: Budget): number {
    if (budget.monthlyLimit === 0) return 0;
    return Math.min(100, (budget.spent / budget.monthlyLimit) * 100);
  }

  isOverBudget(budget: Budget): boolean {
    return budget.spent > budget.monthlyLimit;
  }
}
