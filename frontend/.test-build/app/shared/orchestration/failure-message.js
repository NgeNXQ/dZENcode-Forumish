"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.failureMessage = failureMessage;
const api_failure_1 = require("./api-failure");
function failureMessage(error) {
    return error instanceof api_failure_1.ApiFailure ? error.message : "Something went wrong. Please try again.";
}
