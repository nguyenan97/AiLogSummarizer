import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SummaryService, LogSummary } from '../core/services/summary.service';

@Component({
  selector: 'app-summary-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <h2>Summarize Logs</h2>
  <form (ngSubmit)="submit()">
    <input name="service" [(ngModel)]="model.service" placeholder="Service" required />
    <input name="host" [(ngModel)]="model.host" placeholder="Host" required />
    <input type="date" name="from" [(ngModel)]="model.from" />
    <input type="date" name="to" [(ngModel)]="model.to" />
    <textarea name="rawLog" [(ngModel)]="model.rawLog" placeholder="Paste logs here"></textarea>
    <button type="submit">Summarize</button>
  </form>
  <div *ngIf="summary">
    <h3>Root Cause</h3>
    <p>{{ summary.rootCause }}</p>
    <h3>Key Errors</h3>
    <ul><li *ngFor="let e of summary.keyErrors">{{ e }}</li></ul>
    <h3>Fix Suggestions</h3>
    <ul><li *ngFor="let f of summary.fixSuggestions">{{ f }}</li></ul>
  </div>
  `
})
export class SummaryPanelComponent {
  model = { service: '', host: '', from: '', to: '', rawLog: '' };
  summary?: LogSummary;
  constructor(private summaryService: SummaryService) {}

  async submit() {
    this.summary = await this.summaryService.summarize(this.model);
  }
}
