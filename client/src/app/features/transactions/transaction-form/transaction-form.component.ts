import { Component, OnChanges, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Category, Transaction, TransactionRequest, TransactionType } from '../../../core/models/models';

@Component({
  selector: 'app-transaction-form',
  imports: [ReactiveFormsModule],
  templateUrl: './transaction-form.component.html',
  styleUrl: './transaction-form.component.scss'
})
export class TransactionFormComponent implements OnChanges {
  private readonly fb = inject(FormBuilder);

  readonly categories = input.required<Category[]>();
  readonly editing = input<Transaction | null>(null);
  readonly errorMessage = input<string | null>(null);

  readonly save = output<TransactionRequest>();
  readonly cancel = output<void>();

  readonly form = this.fb.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    type: ['Expense' as TransactionType, [Validators.required]],
    categoryId: [0, [Validators.required, Validators.min(1)]],
    date: [this.today(), [Validators.required]],
    note: ['']
  });

  ngOnChanges(): void {
    const t = this.editing();
    if (t) {
      this.form.reset({
        amount: t.amount,
        type: t.type,
        categoryId: t.categoryId,
        date: t.date,
        note: t.note ?? ''
      });
    } else {
      this.form.reset({ amount: 0, type: 'Expense', categoryId: this.defaultCategoryId(), date: this.today(), note: '' });
    }
  }

  get filteredCategories(): Category[] {
    return this.categories().filter((c) => c.type === this.form.controls.type.value);
  }

  onTypeChange(): void {
    this.form.controls.categoryId.setValue(this.defaultCategoryId());
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.save.emit(this.form.getRawValue() as TransactionRequest);
  }

  private defaultCategoryId(): number {
    const match = this.categories().find((c) => c.type === this.form.controls.type.value);
    return match?.id ?? 0;
  }

  private today(): string {
    return new Date().toISOString().slice(0, 10);
  }
}
