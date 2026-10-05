import { HttpClient, HttpHeaders, HttpParams } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { catchError, map, Observable, of, throwError } from "rxjs";
import { parseComment } from "./parse-comment";
import { parseCommentPage } from "./parse-comment-page";
import { ApiFailure } from "../../../shared/orchestration/api-failure";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { DiscussionGateway } from "../orchestration/discussion-gateway";
import { CaptchaAnswer } from "../orchestration/captcha-answer";
import { Comment } from "../orchestration/comment";
import { Draft } from "../orchestration/draft";
import { Page } from "../orchestration/page";
import { Reading } from "../orchestration/reading";

@Injectable({ providedIn: "root" })
export class DiscussionHttpGateway extends DiscussionGateway {
    private readonly http = inject(HttpClient);
    private readonly settings = inject(APP_SETTINGS);
    private readonly url = `${this.settings.apiBaseUrl.replace(/\/$/, "")}/discussion`;

    override readComments(reading: Reading, parentId?: number): Observable<Page<Comment>> {
        let params = new HttpParams({ fromObject: { ...reading } });
        if (parentId !== undefined) params = params.set("parentId", parentId);
        return this.http.get<unknown>(`${this.url}/comments`, { params }).pipe(
            map(response => parseCommentPage(response, reading, parentId ?? null)),
        );
    }

    override createComment(
        draft: Draft,
        fingerprint: string,
        answer: CaptchaAnswer,
    ): Observable<Comment> {
        const form = new FormData();
        form.set("Email", draft.email.trim());
        form.set("Username", draft.username.trim());
        form.set("Message", draft.message.trim());
        form.set("Fingerprint", fingerprint);
        if (draft.homePage.trim()) form.set("HomePage", draft.homePage.trim());
        if (draft.parentId !== null) form.set("ParentId", String(draft.parentId));
        if (draft.attachment) form.set("Attachment", draft.attachment);
        const headers = new HttpHeaders({
            [this.settings.captcha.idHeader]: answer.id,
            [this.settings.captcha.codeHeader]: answer.code,
        });
        return this.http.post<unknown>(`${this.url}/comments`, form, { headers }).pipe(map(parseComment));
    }

    override readAttachment(id: string): Observable<Blob | null> {
        return this.http
            .get(this.attachmentUrl(id), { responseType: "blob" })
            .pipe(
                catchError((error: unknown) =>
                    error instanceof ApiFailure && error.status === 404
                        ? of(null)
                        : throwError(() => error),
                ),
            );
    }

    override attachmentUrl(id: string): string {
        return `${this.url}/attachments/${encodeURIComponent(id)}`;
    }
}
