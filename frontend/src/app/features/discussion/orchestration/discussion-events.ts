import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { Comment } from "./comment";

@Injectable({ providedIn: "root" })
export class DiscussionEvents {
    private readonly created = new Subject<Comment>();
    readonly created$ = this.created.asObservable();
    announce(comment: Comment): void {
        this.created.next(comment);
    }
}
