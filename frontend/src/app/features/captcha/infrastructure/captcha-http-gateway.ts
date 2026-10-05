import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { map, Observable } from "rxjs";
import { APP_SETTINGS } from "../../../shared/orchestration/app-settings.token";
import { CaptchaGateway } from "../orchestration/captcha-gateway";
import { Challenge } from "../orchestration/challenge";
import { parseChallenge } from "./parse-challenge";

@Injectable({ providedIn: "root" })
export class CaptchaHttpGateway extends CaptchaGateway {
    private readonly http = inject(HttpClient);
    private readonly settings = inject(APP_SETTINGS);

    override create(): Observable<Challenge> {
        const { width, height } = this.settings.captcha;
        return this.http
            .post<unknown>(`${this.settings.apiBaseUrl}/captcha/`, { width, height })
            .pipe(
                map(parseChallenge),
            );
    }
}
