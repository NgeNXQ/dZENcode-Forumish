import { Routes } from '@angular/router';

const page = () => import('./features/discussion/presentation/pages/comments-page.component')
  .then(module => module.CommentsPageComponent);

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'comments' },
  { path: 'comments', loadComponent: page, title: 'Comments · Forumish' },
  { path: '**', redirectTo: 'comments' },
];
