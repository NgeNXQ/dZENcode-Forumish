import { Observable } from "rxjs";

export abstract class FingerprintProvider {
    abstract identify(): Observable<string>;
}
