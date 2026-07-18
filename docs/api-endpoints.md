# API Endpoints

SocialConnect exposes a RESTful API wrapped in a standardized response model.

The base URL for all endpoints is `/api/v1/`.

---

## 📦 Standard API Wrapper

All general responses and errors are wrapped in a standard format using the `ApiResponse<T>` wrapper:

### Success Example
```json
{
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "username": "johndoe"
  },
  "isSuccess": true,
  "errors": null
}
```

### Error Example (e.g., Validation Failure)
```json
{
  "data": null,
  "isSuccess": false,
  "errors": [
    "Email is already in use.",
    "Password must contain at least one uppercase letter."
  ]
}
```

---

## 🔐 Authentication (`/auth`)

*   `POST /auth/register`: Registers a new user account and returns JWT access & refresh tokens.
*   `POST /auth/login`: Authenticates a user with email and password, returning tokens.
*   `POST /auth/refresh-token`: Requests a new JWT access token using a valid refresh token.
*   `POST /auth/logout`: Invalidates the user's active refresh token session.
*   `POST /auth/revoke-token`: Revokes a specific refresh token (Admin/User security hardening).
*   `POST /auth/verify-email`: Validates an email verification token.
*   `POST /auth/forgot-password`: Generates and triggers a password recovery token email.
*   `POST /auth/reset-password`: Resets a user password using a recovery token.
*   `POST /auth/change-password`: Changes the authenticated user's password.

---

## 👤 Users & Profiles (`/users`)

*   `GET /users/me`: Gets the profile details of the currently authenticated user.
*   `PUT /users/me`: Updates profile details (`FirstName`, `LastName`, `Headline`, `Bio`, `Location`, `DateOfBirth`).
*   `POST /users/me/profile-image`: Uploads a profile avatar (validates type/dimension bounds; automatically prunes old S3 assets).
*   `POST /users/me/cover-image`: Uploads a profile background cover photo.
*   `GET /users/search`: Offset-paginated case-insensitive profile search using a query string.
*   `GET /users/{userId}`: Retrieves profile details of a specific user.
*   `GET /users/{userId}/details`: Retrieves detailed account + profile information for a specific user ID.

---

## 📝 Posts & Media (`/posts`)

*   `POST /posts`: Creates a new post (supports draft mode, scheduled times, and body content).
*   `POST /posts/media`: Uploads media attachments (images/videos) to AWS S3 storage.
*   `GET /posts/search`: Queries published posts (automatically filtering out content from blocked/muted authors).
*   `POST /posts/{id}/publish`: Publishes a draft or scheduled post immediately.
*   `POST /posts/{id}/schedule`: Schedules a draft post for future publication.

---

## 💬 Comments & Reactions (`/comments`, `/reactions`)

*   `POST /comments`: Adds a comment or sub-reply to a post.
*   `PUT /comments/{id}`: Edits comment text content (author-only check).
*   `DELETE /comments/{id}`: Soft-deletes a comment.
*   `GET /comments/post/{postId}`: Retrieves paginated comments/replies for a specific post.
*   `POST /reactions/toggle`: Toggles reactions (Like, Love, Haha, Wow, Sad, Angry) on posts or comments.
*   `GET /reactions`: Retrieves paginated lists of reactions mapped to users for a specific target ID.

---

## 💬 Messaging & Real-Time Chat (`/messages`)

*   `POST /messages/conversation`: Opens a one-to-one conversation with another user.
*   `POST /messages/group`: Creates a group conversation naming admins and members.
*   `GET /messages/conversations`: Retrieves all active chat rooms showing last sent messages, member lists, and unread counts.
*   `POST /messages`: Sends a text message or file attachment within a conversation.
*   `PUT /messages/{id}`: Modifies a sent message (author-only check).
*   `DELETE /messages/{id}`: Soft-deletes a sent message.
*   `POST /messages/conversation/{id}/read`: Marks all messages within a conversation as read.
*   `GET /messages/conversation/{conversationId}`: Retrieves reverse-chronological paginated messages for a chat room.

---

## 🤝 Social Connections (`/social`)

*   `POST /social/follow/{userId}`: Follows a target user.
*   `POST /social/unfollow/{userId}`: Unfollows a target user.
*   `POST /social/block/{userId}`: Blocks a user (automatically destroys active mutual follow bounds).
*   `POST /social/unblock/{userId}`: Unblocks a user.
*   `POST /social/mute/{userId}`: Mutes a user (hides posts in feed queries).
*   `POST /social/unmute/{userId}`: Unmutes a user.
*   `GET /social/followers/{userId}`: Lists followers for a user.
*   `GET /social/following/{userId}`: Lists users followed by a user.

---

## 🔔 Notifications (`/notifications`)

*   `GET /notifications`: Retrieves a paginated list of notifications.
*   `POST /notifications/read-all`: Marks all notifications as read.
