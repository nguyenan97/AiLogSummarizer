import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LogService {
  async upload(file: File): Promise<{ count: number; preview: string }> {
    const form = new FormData();
    form.append('file', file);
    const res = await axios.post(`${environment.apiBaseUrl}/api/logs/upload`, form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    });
    return res.data;
  }
}
