import { Component, OnInit, signal } from '@angular/core';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransactionService } from '../../../core/services/transaction.service';
import { CategoryService } from '../../../core/services/category.service';
import { Category, Transaction, TransactionRequest, TransactionType } from '../../../core/models/models';
import { TransactionFormComponent } from '../transaction-form/transaction-form.component';

@Component({
  selector: 'app-transaction-list',
  imports: [CurrencyPipe, DatePipe, FormsModule, TransactionFormComponent],
  templateUrl: './transaction-list.component.html',
  styleUrl: './transaction-list.component.scss'
})
export class TransactionListComponent implements OnInit {
  readonly transactions = signal<Transaction[]>([]);
  readonly categories = signal<Category[]>([]);
  readonly totalCount = signal(0);
  readonly page = signal(1);
  readonly pageSize = 15;
  readonly loading = signal(true);
  readonly showForm = signal(false);
  readonly editing = signal<Transaction | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly importResult = signal<{ imported: number; skipped: number; errors: string[] } | null>(null);
  readonly importing = signal(false);

  filterCategoryId: number | null = null;
  filterType: TransactionType | null = null;

  constructor(
    private transactionService: TransactionService,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.categoryService.getAll().subscribe((categories) => this.categories.set(categories));
    this.load();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
  }

  load(): void {
    this.loading.set(true);
    this.transactionService
      .getAll({
        categoryId: this.filterCategoryId ?? undefined,
        type: this.filterType ?? undefined,
        page: this.page(),
        pageSize: this.pageSize
      })
      .subscribe((result) => {
        this.transactions.set(result.items);
        this.totalCount.set(result.totalCount);
        this.loading.set(false);
      });
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.page.set(page);
    this.load();
  }

  startCreate(): void {
    this.editing.set(null);
    this.errorMessage.set(null);
    this.showForm.set(true);
  }

  startEdit(transaction: Transaction): void {
    this.editing.set(transaction);
    this.errorMessage.set(null);
    this.showForm.set(true);
  }

  cancelForm(): void {
    this.showForm.set(false);
  }

  saveTransaction(request: TransactionRequest): void {
    const editing = this.editing();
    const action = editing
      ? this.transactionService.update(editing.id, request)
      : this.transactionService.create(request);

    action.subscribe({
      next: () => {
        this.showForm.set(false);
        this.load();
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Could not save transaction.')
    });
  }

  remove(transaction: Transaction): void {
    if (!confirm('Delete this transaction?')) return;
    this.transactionService.delete(transaction.id).subscribe(() => this.load());
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.importing.set(true);
    this.importResult.set(null);

    this.transactionService.importCsv(file).subscribe({
      next: (result) => {
        this.importing.set(false);
        this.importResult.set(result);
        this.load();
        input.value = '';
      },
      error: () => {
        this.importing.set(false);
        this.importResult.set({ imported: 0, skipped: 0, errors: ['Import failed.'] });
        input.value = '';
      }
    });
  }
}
