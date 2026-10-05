import { bootstrapApplication } from "@angular/platform-browser";
import { appConfig } from "./app/app.config";
import { AppComponent } from "./app/app.component";
import { parseAppSettings } from "./app/shared/orchestration/parse-app-settings";

async function start(): Promise<void> {
    const response = await fetch("/app-config.json", {
        cache: "no-store",
        signal: AbortSignal.timeout(15000),
        redirect: "error",
    });
    if (!response.ok) throw new Error("Application settings could not be loaded.");
    const settings = parseAppSettings(await response.json());
    await bootstrapApplication(AppComponent, appConfig(settings));
}

start().catch(() => {
    const root = document.querySelector("app-root");
    if (root) root.textContent = "Forumish could not start. Please refresh and try again.";
});
