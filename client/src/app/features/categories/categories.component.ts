import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../core/services/category.service';
import { Category, TransactionType } from '../../core/models/models';

const SWATCHES = ['#2a78d6', '#eb6834', '#1baf7a', '#eda100', '#e87ba4', '#008300', '#4a3aa7', '#e34948', '#898781'];

@Component({
  selector: 'app-categories',
  imports: [ReactiveFormsModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.scss'
})
export class CategoriesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly categoryService = inject(CategoryService);

  readonly categories = signal<Category[]>([]);
  readonly loading = signal(true);
  readonly showForm = signal(false);
  readonly editingId = signal<number | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly swatches = SWATCHES;

  readonly form = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(1)]],
    type: ['Expense' as TransactionType, [Validators.required]],
    color: [SWATCHES[0], [Validators.required]],
    icon: ['tag']
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.categoryService.getAll().subscribe((categories) => {
      this.categories.set(categories);
      this.loading.set(false);
    });
  }

  startCreate(): void {
    this.editingId.set(null);
    this.form.reset({ name: '', type: 'Expense', color: SWATCHES[0], icon: 'tag' });
    this.showForm.set(true);
    this.errorMessage.set(null);
  }

  startEdit(category: Category): void {
    this.editingId.set(category.id);
    this.form.reset({ name: category.name, type: category.type, color: category.color, icon: category.icon });
    this.showForm.set(true);
    this.errorMessage.set(null);
  }

  cancel(): void {
    this.showForm.set(false);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request = this.form.getRawValue() as { name: string; type: TransactionType; color: string; icon: string };
    const id = this.editingId();
    const action = id ? this.categoryService.update(id, request) : this.categoryService.create(request);

    action.subscribe({
      next: () => {
        this.showForm.set(false);
        this.load();
      },
      error: (err) => this.errorMessage.set(err.error?.message ?? 'Could not save category.')
    });
  }

  remove(category: Category): void {
    if (!confirm(`Delete category "${category.name}"?`)) return;

    this.categoryService.delete(category.id).subscribe({
      next: () => this.load(),
      error: (err) => alert(err.error?.message ?? 'Could not delete category.')
    });
  }
}
