# Swagger UI Password Protection

## Overview
A simple password protection feature has been implemented for the Swagger UI to restrict access. The feature displays a modal popup before the Swagger page loads, requiring users to enter a password for verification.

## Functionality
- **Password Prompt**: A modal dialog appears on page load, prompting the user to enter a password.
- **Correct Password**: The password is "123" (intended for testing purposes).
- **Verification**: Passwords are verified using SHA-256 hashing for security.
- **Error Handling**: If the password is incorrect, an alert message "Incorrect password" is displayed, allowing retry.
- **UI**: The modal is minimalistic with a title "Swagger Access", note indicating "123 for testing this feature", and styled input/button elements. The background is solid black to completely hide the underlying Swagger UI content for security.

## Technical Implementation
- **Location**: Implemented in `src/WebApi/Program.cs` within the Swagger UI configuration.
- **Library**: Uses CryptoJS library loaded from CDN (https://cdnjs.cloudflare.com/ajax/libs/crypto-js/4.1.1/crypto-js.min.js) for SHA-256 hashing.
- **Code Integration**: JavaScript code is injected via `SwaggerUIOptions.HeadContent` to ensure it runs on page load without altering existing application logic.
- **Hash Comparison**: The SHA-256 hash of "123" is `a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3`.

## Usage
Access the Swagger UI at the configured endpoint (e.g., `/swagger`). Enter "123" when prompted to proceed. For production, update the password and hash accordingly.