"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.ApiFailure = void 0;
class ApiFailure extends Error {
    status;
    fields;
    retryAfterSeconds;
    constructor(status, message, fields = {}, retryAfterSeconds = null) {
        super(message);
        this.status = status;
        this.fields = fields;
        this.retryAfterSeconds = retryAfterSeconds;
    }
}
exports.ApiFailure = ApiFailure;
