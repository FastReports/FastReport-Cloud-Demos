import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HttpHeaders } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { RouterModule } from '@angular/router';
import { switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [FormsModule, HttpClientModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})

@Injectable()
export class AppComponent {
  title = 'cloud-demo-angular';

  constructor(private http: HttpClient) { }

  host = 'https://fastreport.cloud';
  subId = '';
  apiKey = '';
  templateId = '';
  exportId = '';
  templateFolder = '';
  exportFolder = '';
  parameterKey = 'Parameter1';
  parameterValue = '';

  setAuthorizationHeader() {
    return new HttpHeaders({
      'Authorization': `Basic ${btoa(`apikey:${this.apiKey}`)}`
    });
  }

  getTemplateRoot() {
    const headers = this.setAuthorizationHeader();
    return this.http.get(
      `${this.host}/api/rp/v1/Templates/Root?subscriptionId=${this.subId}`,
      { headers: headers }
    ).pipe(
      map((response: any) => response.id)
    );
  }

  getExportRoot() {
    const headers = this.setAuthorizationHeader();
    return this.http.get(
      `${this.host}/api/rp/v1/Exports/Root?subscriptionId=${this.subId}`,
      { headers: headers }
    ).pipe(
      map((response: any) => response.id)
    );
  }

  onUploadButtonClick() {
    this.getTemplateRoot().pipe(
      switchMap((data: any) => {
        this.templateFolder = data;
        return this.http.get('assets/Template.frx', { responseType: 'text' });
      }),
      switchMap((data: any) => {
        const fileContent = new Blob([data], { type: 'application/json' });
        const formData = new FormData();
        formData.append('FileContent', fileContent, 'Template.frx');

        const url = `${this.host}/api/rp/v2/Templates/Folder/${this.templateFolder}/File`;

        const headers = this.setAuthorizationHeader();
        headers.set('Accept', 'application/json');
        headers.set('Content-Type', fileContent.type);

        return this.http.post(url, formData, { headers }).pipe(
          map((response: any) => response.id)
        );
      })
    ).subscribe((data: any) => {
      this.templateId = data;
      console.log('Success uploading!');
    });
  }

  onExportButtonClick() {
    this.getExportRoot()
      .pipe(
        switchMap((data: any) => {
          this.exportFolder = data;
          const url = `${this.host}/api/rp/v1/Templates/File/${this.templateId}/Export`;

          const jsonRequest = {
            '$t': 'ExportTemplateVM',
            'fileName': 'Exported file',
            'format': 'Pdf',
            'folderId': this.exportFolder,
            'reportParameters': {
              [this.parameterKey]: this.parameterValue
            }
          };

          const body = JSON.stringify(jsonRequest);
          const headers = this.setAuthorizationHeader().set('Content-Type', 'application/json');

          return this.http.post(url, body, { headers });
        }),
        map((response: any) => response.id)
      )
      .subscribe((data: any) => {
        this.exportId = data;
        console.log('Success exporting!');
      });
  }

  onDownloadButtonClick() {
    const url = `${this.host}/download/e/${this.exportId}?preview=false`;

    const headers = this.setAuthorizationHeader();
    headers.set('Accept', 'text/plain');

    this.http.get(url, { responseType: 'blob', headers: headers }).subscribe(blob => {
      const downloadUrl = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = 'Exported file.pdf';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      console.log('Succes downloading!');
    });
  }

}
