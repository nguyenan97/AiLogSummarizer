import { Injectable } from '@angular/core';
import axios from 'axios';
import { environment } from '../../../environments/environment';

export interface SummarizeRequest {
  service: string;
  host: string;
  from?: string;
  to?: string;
  rawLog: string;
}

export interface LogSummary {
  rootCause: string;
  keyErrors: string[];
  fixSuggestions: string[];
}

@Injectable({ providedIn: 'root' })
export class SummaryService {
  async summarize(req: SummarizeRequest): Promise<LogSummary> {
    const res = await axios.post(`${environment.apiBaseUrl}/api/summaries`, req);
    return res.data;
  }
}
