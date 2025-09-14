import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-log-viewer',
  standalone: true,
  imports: [CommonModule],
  template: `<pre>{{ content }}</pre>`
})
export class LogViewerComponent {
  @Input({ required: true }) content = '';
}
