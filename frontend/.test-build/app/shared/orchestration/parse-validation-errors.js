"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseValidationErrors = parseValidationErrors;
const is_record_1 = require("./is-record");
function parseValidationErrors(value) {
    if (!(0, is_record_1.isRecord)(value))
        return {};
    return Object.fromEntries(Object.entries(value).slice(0, 50).flatMap(([field, messages]) => {
        if (!Array.isArray(messages))
            return [];
        const safeMessages = messages.filter((message) => typeof message === "string" && message.length <= 2048).slice(0, 20);
        return safeMessages.length ? [[field, safeMessages]] : [];
    }));
}
