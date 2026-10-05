import { AsyncPipe } from "@angular/common";
import { Component, effect, inject, input } from "@angular/core";
import { AttachmentFacade } from "../../orchestration/attachment-facade";

@Component({
    selector: "app-attachment-preview",
    imports: [AsyncPipe],
    providers: [AttachmentFacade],
    template: `
        @if (facade.state$ | async; as state) {
            <div class="attachment-preview">
                @if (state.loading) {
                    <p class="muted" role="status">◌ Preparing attachment…</p>
                }
                @if (state.imageUrl) {
                    <a [href]="state.openUrl" target="_blank" rel="noopener noreferrer">
                        <img
                            [src]="state.imageUrl"
                            alt="Image attached to this comment"
                            loading="lazy"
                        />
                    </a>
                }
                @if (state.text !== null) {
                    <div class="text-attachment">
                        <span class="tiny-label">TEXT ATTACHMENT</span>
                        <pre>{{ state.text }}</pre>
                    </div>
                }
                @if (state.unavailable || state.error) {
                    <p class="muted">
                        {{ state.error || "This file is not available yet or was rejected." }}
                    </p>
                    <button type="button" class="text-button" (click)="facade.load(id())">
                        Check again
                    </button>
                }
                @if (state.imageUrl || state.text !== null) {
                    <a
                        class="attachment-link"
                        [href]="state.openUrl"
                        target="_blank"
                        rel="noopener noreferrer"
                    >
                        ↗ Open attachment
                    </a>
                }
            </div>
        }
    `,
})
export class AttachmentPreviewComponent {
    readonly id = input.required<string>();
    readonly facade = inject(AttachmentFacade);
    constructor() {
        effect(() => this.facade.load(this.id()));
    }
}
