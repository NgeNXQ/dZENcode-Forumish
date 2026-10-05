import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterOutlet],
  template: `
    <a class="skip-link" href="#main">Skip to content</a>
    <header class="site-header">
      <div class="header-inner">
        <a class="brand" routerLink="/comments" aria-label="Forumish home">
          <span class="brand-mark">f<span>·</span></span> forumish<span class="brand-dot">.</span>
        </a>
      </div>
    </header>
    <router-outlet />
  `,
})
export class AppComponent {}
