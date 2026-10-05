import { AsyncPipe, DatePipe } from "@angular/common";
import { Component, effect, forwardRef, inject, input, output } from "@angular/core";
import { Comment } from "../../orchestration/comment";
import { CommentTreeFacade } from "../../orchestration/comment-tree-facade";
import { AttachmentPreviewComponent } from "./attachment-preview.component";

@Component({
    selector: "app-comment-card",
    imports: [
        AsyncPipe,
        DatePipe,
        AttachmentPreviewComponent,
        forwardRef(() => CommentCardComponent),
    ],
    providers: [CommentTreeFacade],
    template: `
        <article
            [id]="'comment-' + comment().id"
            class="comment-card"
            [class.reply-card]="depth() > 0"
        >
            <div class="comment-top">
                <div class="comment-identity">
                    <div class="author-line">
                        <strong>{{ comment().username }}</strong>
                        <a class="email-link" [href]="'mailto:' + comment().email">{{ comment().email }}</a>
                        @if (comment().homePage) {
                            <a class="website-link" [href]="comment().homePage" target="_blank"
                                rel="noopener noreferrer">{{ comment().homePage }}</a>
                        }
                        <time [attr.datetime]="comment().createdAt">
                            {{ comment().createdAt | date: "MM/dd/yy HH:mm" }}
                        </time>
                        <a class="comment-permalink" [href]="'#comment-' + comment().id">No.{{ comment().id }}</a>
                    </div>
                </div>
            </div>
            <div class="comment-body" [innerHTML]="comment().message"></div>
            @if (comment().attachmentId; as attachment) {
                <app-attachment-preview [id]="attachment" />
            }
            <div class="comment-actions">
                <button type="button" class="text-button" (click)="reply.emit(comment())">
                    [Reply]
                </button>
                @if (facade.state$ | async; as state) {
                    <button
                        type="button"
                        class="text-button muted"
                        (click)="facade.toggle()"
                        [attr.aria-expanded]="state.expanded"
                    >
                        {{ state.expanded ? "[Hide replies]" : "[Show replies]" }}
                    </button>
                }
            </div>
            @if (facade.state$ | async; as state) {
                @if (state.expanded) {
                    <div class="thread" [class.thread-flat]="depth() >= 3">
                        @for (child of state.items; track child.id) {
                            <app-comment-card
                                [comment]="child"
                                [depth]="depth() + 1"
                                (reply)="reply.emit($event)"
                            />
                        }
                        @if (state.loading) {
                            <p class="muted" role="status">Loading replies…</p>
                        }
                        @if (!state.loading && state.loaded && state.items.length === 0) {
                            <p class="empty-replies">
                                No replies.
                            </p>
                        }
                        @if (state.error) {
                            <p class="field-error" role="alert">{{ state.error }}</p>
                            <button type="button" class="text-button" (click)="facade.retry()">
                                Try again
                            </button>
                        }
                        @if (state.hasNext && !state.error) {
                            <button
                                type="button"
                                class="text-button"
                                (click)="facade.more()"
                                [disabled]="state.loading"
                            >
                                Load more replies ↓
                            </button>
                        }
                    </div>
                }
            }
        </article>
    `,
})
export class CommentCardComponent {
    readonly comment = input.required<Comment>();
    readonly depth = input(0);
    readonly reply = output<Comment>();
    readonly facade = inject(CommentTreeFacade);
    constructor() {
        effect(() => this.facade.bind(this.comment().id));
    }
}
