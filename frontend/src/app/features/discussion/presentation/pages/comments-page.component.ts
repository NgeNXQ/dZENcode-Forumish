import { AsyncPipe } from "@angular/common";
import { afterNextRender, Component, ElementRef, inject, Injector, signal, viewChild } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { FormsModule } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";
import { APP_SETTINGS } from "../../../../shared/orchestration/app-settings.token";
import { DiscussionEvents } from "../../orchestration/discussion-events";
import { CommentsFacade } from "../../orchestration/comments-facade";
import { Comment } from "../../orchestration/comment";
import { Reading } from "../../orchestration/reading";
import { CommentCardComponent } from "../components/comment-card.component";
import { CommentComposerComponent } from "../components/comment-composer.component";

@Component({
    selector: "app-comments-page",
    imports: [AsyncPipe, FormsModule, CommentCardComponent, CommentComposerComponent],
    providers: [CommentsFacade],
    templateUrl: "./comments-page.component.html",
})
export class CommentsPageComponent {
    readonly facade = inject(CommentsFacade);
    private readonly route = inject(ActivatedRoute);
    private readonly router = inject(Router);
    private readonly events = inject(DiscussionEvents);
    private readonly injector = inject(Injector);
    readonly settings = inject(APP_SETTINGS);
    readonly replyingTo = signal<Comment | null>(null);
    readonly postedComment = signal<Comment | null>(null);
    readonly composerMounted = signal(false);
    readonly reading = signal<Reading>({
        page: 1,
        size: this.settings.comment.defaultPageSize,
        sortBy: "CreatedAt",
        direction: "Descending",
    });
    private readonly composer = viewChild(CommentComposerComponent);
    private readonly composerDialog = viewChild<ElementRef<HTMLDialogElement>>("composerDialog");

    get composerBusy(): boolean {
        return this.composer()?.busy() ?? false;
    }

    constructor() {
        this.route.queryParamMap.pipe(takeUntilDestroyed()).subscribe((query) => {
            const page = Number(query.get("page") ?? 1);
            const size = Number(query.get("size") ?? this.settings.comment.defaultPageSize);
            const sortBy = query.get("sortBy");
            const direction = query.get("direction");
            this.reading.set({
                page: Number.isSafeInteger(page) && page > 0 ? page : 1,
                size:
                    Number.isSafeInteger(size) &&
                    size > 0 &&
                    size <= this.settings.comment.maximumPageSize
                        ? size
                        : this.settings.comment.defaultPageSize,
                sortBy: sortBy === "Email" || sortBy === "Username" ? sortBy : "CreatedAt",
                direction: direction === "Ascending" ? "Ascending" : "Descending",
            });
            this.reload();
        });
        this.events.created$.pipe(takeUntilDestroyed()).subscribe((comment) => {
            if (comment.parentId === null) this.reload();
        });
    }

    reload(): void {
        this.facade.load(this.reading());
    }

    changeSorting(event: Event, field: "sortBy" | "direction"): void {
        if (!(event.target instanceof HTMLSelectElement)) return;
        const value = event.target.value;
        const update: Partial<Reading> = field === "sortBy"
            ? { sortBy: value === "Email" || value === "Username" ? value : "CreatedAt" }
            : { direction: value === "Ascending" ? "Ascending" : "Descending" };
        this.navigate({ ...this.reading(), ...update, page: 1 });
    }

    page(page: number): void {
        this.navigate({ ...this.reading(), page });
    }
    changePageSize(size: number): void {
        this.navigate({ ...this.reading(), size, page: 1 });
    }
    private navigate(reading: Reading): void {
        void this.router.navigate([], { relativeTo: this.route, queryParams: reading });
    }
    reply(comment: Comment): void {
        this.replyingTo.set(comment);
        this.openComposer();
    }
    newComment(): void {
        this.replyingTo.set(null);
        this.openComposer();
    }
    private openComposer(): void {
        this.composerMounted.set(true);
        afterNextRender(() => {
            if (!this.composerMounted()) return;
            const dialog = this.composerDialog()?.nativeElement;
            if (dialog && !dialog.open) dialog.showModal();
            this.composer()?.focus();
        }, { injector: this.injector });
    }
    closeComposer(): void {
        if (this.composerBusy) return;
        this.composerDialog()?.nativeElement.close();
        this.resetComposer();
    }
    resetComposer(): void {
        this.composerMounted.set(false);
        this.replyingTo.set(null);
    }
    cancelComposer(event: Event): void {
        event.preventDefault();
        this.closeComposer();
    }
    onPosted(comment: Comment): void {
        this.postedComment.set(comment);
        this.composerDialog()?.nativeElement.close();
        this.resetComposer();
    }
}
