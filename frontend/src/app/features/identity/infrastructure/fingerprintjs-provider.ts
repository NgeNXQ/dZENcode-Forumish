import { Injectable } from "@angular/core";
import { defer, Observable, shareReplay } from "rxjs";
import { FingerprintProvider } from "../orchestration/fingerprint-provider";

@Injectable({ providedIn: "root" })
export class FingerprintJsProvider extends FingerprintProvider {
    private readonly fingerprint$ = defer(async () => {
        const fingerprint = await import("@fingerprintjs/fingerprintjs");
        const agent = await fingerprint.load({ monitoring: false });
        return (await agent.get()).visitorId;
    }).pipe(shareReplay({ bufferSize: 1, refCount: false }));

    override identify(): Observable<string> {
        return this.fingerprint$;
    }
}
