import { Observable } from "rxjs";
import { CaptchaAnswer } from "./captcha-answer";
import { Comment } from "./comment";
import { Draft } from "./draft";
import { Page } from "./page";
import { Reading } from "./reading";

export abstract class DiscussionGateway {
    abstract readComments(reading: Reading, parentId?: number): Observable<Page<Comment>>;
    abstract createComment(
        draft: Draft,
        fingerprint: string,
        answer: CaptchaAnswer,
    ): Observable<Comment>;
    abstract readAttachment(id: string): Observable<Blob | null>;
    abstract attachmentUrl(id: string): string;
}
