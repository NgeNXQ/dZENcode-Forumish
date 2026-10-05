import { AttachmentState } from "./attachment-state";
import { DestroyRef, inject, Injectable } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import {
    BehaviorSubject,
    catchError,
    defaultIfEmpty,
    EMPTY,
    filter,
    of,
    repeat,
    Subject,
    switchMap,
    take,
    tap,
} from "rxjs";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { failureMessage } from "../../../shared/orchestration/failure-message";
import { DiscussionGateway } from "./discussion-gateway";
import { AttachmentPreview } from "./attachment-preview";
import { AttachmentPreviewProvider } from "./attachment-preview-provider";


@Injectable()
export class AttachmentFacade {
    private readonly gateway = inject(DiscussionGateway);
    private readonly previews = inject(AttachmentPreviewProvider);
    private readonly settings = inject(APP_SETTINGS).attachment;
    private readonly requests = new Subject<string>();
    private readonly state = new BehaviorSubject<AttachmentState>({
        loading: true,
        error: null,
        imageUrl: null,
        text: null,
        openUrl: "",
        unavailable: false,
    });
    private preview: AttachmentPreview | null = null;
    readonly state$ = this.state.asObservable();

    constructor() {
        inject(DestroyRef).onDestroy(() => this.revoke());
        this.requests
            .pipe(
                tap((id) => {
                    this.revoke();
                    this.state.next({
                        loading: true,
                        error: null,
                        imageUrl: null,
                        text: null,
                        unavailable: false,
                        openUrl: this.gateway.attachmentUrl(id),
                    });
                }),
                switchMap((id) =>
                    this.gateway.readAttachment(id).pipe(
                        repeat({ count: this.settings.pollAttempts, delay: this.settings.pollIntervalMs }),
                        filter((blob): blob is Blob => blob !== null),
                        take(1),
                        defaultIfEmpty(null),
                        switchMap((blob) => {
                            if (blob === null)
                                return of({ imageUrl: null, text: null, unavailable: true });
                            return this.previews.prepare(blob);
                        }),
                        tap((content) => {
                            this.preview = content;
                            this.state.next({ ...this.state.value, ...content, loading: false });
                        }),
                        catchError((error) => {
                            this.state.next({
                                ...this.state.value,
                                loading: false,
                                error: failureMessage(error),
                            });
                            return EMPTY;
                        }),
                    ),
                ),
                takeUntilDestroyed(),
            )
            .subscribe();
    }

    load(id: string): void {
        this.requests.next(id);
    }
    private revoke(): void {
        if (this.preview) this.previews.release(this.preview);
        this.preview = null;
    }
}
