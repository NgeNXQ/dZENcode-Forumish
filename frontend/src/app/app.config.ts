import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { routes } from './app.routes';
import { AppSettings } from './shared/orchestration/app-settings';
import { APP_SETTINGS } from './shared/orchestration/app-settings.token';
import { apiErrorInterceptor } from './shared/infrastructure/api-error.interceptor';
import { DiscussionGateway } from './features/discussion/orchestration/discussion-gateway';
import { DiscussionHttpGateway } from './features/discussion/infrastructure/discussion-http-gateway';
import {
  AttachmentPreviewProvider,
} from './features/discussion/orchestration/attachment-preview-provider';
import {
  BrowserAttachmentPreviewProvider,
} from './features/discussion/infrastructure/browser-attachment-preview-provider';
import { CaptchaGateway } from './features/captcha/orchestration/captcha-gateway';
import { CaptchaHttpGateway } from './features/captcha/infrastructure/captcha-http-gateway';
import { FingerprintProvider } from './features/identity/orchestration/fingerprint-provider';
import { FingerprintJsProvider } from './features/identity/infrastructure/fingerprintjs-provider';

export function appConfig(settings: AppSettings): ApplicationConfig {
  return { providers: [
    { provide: APP_SETTINGS, useValue: settings },
    provideHttpClient(withInterceptors([apiErrorInterceptor])),
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'enabled' })),
    { provide: DiscussionGateway, useExisting: DiscussionHttpGateway },
    { provide: AttachmentPreviewProvider, useExisting: BrowserAttachmentPreviewProvider },
    { provide: CaptchaGateway, useExisting: CaptchaHttpGateway },
    { provide: FingerprintProvider, useExisting: FingerprintJsProvider },
  ] };
}
