import { CommentTreeState } from "./comment-tree-state";
import { CommentTreeRequest } from "./comment-tree-request";
import { inject, Injectable } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { BehaviorSubject, catchError, EMPTY, Subject, switchMap, tap } from "rxjs";
import { failureMessage } from "../../../shared/orchestration/failure-message";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { DiscussionGateway } from "./discussion-gateway";
import { DiscussionEvents } from "./discussion-events";


const initialState = (): CommentTreeState => ({
    expanded: false,
    loaded: false,
    loading: false,
    error: null,
    items: [],
    page: 0,
    hasNext: false,
});

@Injectable()
export class CommentTreeFacade {
    private readonly gateway = inject(DiscussionGateway);
    private readonly events = inject(DiscussionEvents);
    private readonly pageSize = inject(APP_SETTINGS).comment.defaultPageSize;
    private readonly requests = new Subject<CommentTreeRequest | null>();
    private readonly state = new BehaviorSubject<CommentTreeState>(initialState());
    private parentId = 0;
    private requestedPage = 1;
    readonly state$ = this.state.asObservable();

    constructor() {
        this.requests
            .pipe(
                switchMap((request) => {
                    if (request === null) return EMPTY;
                    this.requestedPage = request.page;
                    this.state.next({ ...this.state.value, loading: true, error: null });
                    return this.gateway
                        .readComments(
                            {
                                page: request.page,
                                size: this.pageSize,
                                sortBy: "CreatedAt",
                                direction: "Ascending",
                            },
                            request.parentId,
                        )
                        .pipe(
                            tap((result) => {
                                const items =
                                    request.page === 1
                                        ? result.items
                                        : [...this.state.value.items, ...result.items];
                                this.state.next({
                                    ...this.state.value,
                                    loading: false,
                                    loaded: true,
                                    page: result.page,
                                    items: [
                                        ...new Map(items.map((item) => [item.id, item])).values(),
                                    ],
                                    hasNext: result.items.length === result.size,
                                });
                            }),
                            catchError((error) => {
                                this.state.next({
                                    ...this.state.value,
                                    loading: false,
                                    error: failureMessage(error),
                                });
                                return EMPTY;
                            }),
                        );
                }),
                takeUntilDestroyed(),
            )
            .subscribe();
        this.events.created$.pipe(takeUntilDestroyed()).subscribe((comment) => {
            if (comment.parentId === this.parentId) {
                this.state.next({ ...this.state.value, expanded: true });
                this.load(1);
            }
        });
    }

    bind(parentId: number): void {
        if (this.parentId === parentId) return;
        this.requests.next(null);
        this.parentId = parentId;
        this.requestedPage = 1;
        this.state.next(initialState());
    }
    toggle(): void {
        const expanded = !this.state.value.expanded;
        this.state.next({ ...this.state.value, expanded });
        if (expanded && !this.state.value.loaded && !this.state.value.loading) this.load(1);
    }
    more(): void {
        if (!this.state.value.loading && this.state.value.hasNext)
            this.load(this.state.value.page + 1);
    }
    retry(): void {
        if (!this.state.value.loading) this.load(this.requestedPage);
    }
    private load(page: number): void {
        this.requests.next({ parentId: this.parentId, page });
    }
}
