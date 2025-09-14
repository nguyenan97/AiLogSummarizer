import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogService } from '../core/services/log.service';
import { LogViewerComponent } from '../log-viewer/log-viewer.component';
import { SummaryPanelComponent } from '../summary-panel/summary-panel.component';

@Component({
  selector: 'app-log-upload',
  standalone: true,
  imports: [CommonModule, LogViewerComponent, SummaryPanelComponent],
  template: `
  <h2>Upload Log</h2>
  <input type="file" (change)="onFile($event)" />
  <app-log-viewer [content]="preview"></app-log-viewer>
  <app-summary-panel></app-summary-panel>
  `
})
export class LogUploadComponent {
  preview = '';
  constructor(private logService: LogService) {}

  async onFile(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const result = await this.logService.upload(file);
    this.preview = result.preview;
  }
}
