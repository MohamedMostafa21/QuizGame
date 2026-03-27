# Testing Guide - LeaderboardService & ChatService

## Overview

This guide explains how to test the `LeaderboardService` and `ChatService` using the `TestController`. The test controller provides a web interface and API endpoints to verify all functionality works correctly before integration with GameHub.

---

## Prerequisites

Before testing, ensure:

1. **Database is running** and connection string is configured in `appsettings.json`
2. **Migrations are applied** - The database should have all tables created
3. **At least 2 users exist** in the database (register via `/Account/Register`)
4. **At least 1 game exists** in the database (create via `/Lobby/Create`)
5. **Service registrations added** to `Program.cs`:
   ```csharp
   builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
   builder.Services.AddScoped<ChatService>();
   builder.Services.AddScoped<LeaderboardService>();
   ```
6. **Application is running** on `https://localhost:XXXX` or `http://localhost:XXXX`

---

## Accessing the Test Dashboard

Navigate to: **`/test`** or **`/Test/Index`**

You'll see a dashboard with:
- List of all available test endpoints
- Description of what each test does
- URL and HTTP method for each test
- Whether authentication is required
- Action buttons to execute tests

---

## Test Categories

### 1. Chat Service Tests

#### **Test 1: Save Chat Message**
- **URL:** `/test/chat/save`
- **Method:** POST
- **Auth Required:** Yes (must be logged in)
- **Parameters:** 
  - `gameId` (default: 1)
  - `text` (default: "Test message")

**What it does:**
- Saves a chat message to the database
- Uses the currently logged-in user's ID
- Returns the saved message with ID and timestamp

**Expected Result:**
```json
{
  "success": true,
  "message": "Chat message saved successfully",
  "data": {
    "id": 1,
    "gameId": 1,
    "userId": "user-id",
    "text": "Test message",
    "sentAt": "2025-03-27T..."
  }
}
```

**How to test:**
1. Log in as any user
2. Click "Run" button for this test
3. Check the console output for success response

---

#### **Test 2: Get Chat History**
- **URL:** `/test/chat/history?gameId=1`
- **Method:** GET
- **Auth Required:** No
- **Parameters:** `gameId` (default: 1)

**What it does:**
- Retrieves all chat messages for a specific game
- Orders messages by timestamp (oldest first)
- Includes user information (username) for each message

**Expected Result:**
```json
{
  "success": true,
  "message": "Retrieved 5 messages",
  "data": [
    {
      "id": 1,
      "gameId": 1,
      "userId": "user1",
      "text": "Hello everyone!",
      "sentAt": "2025-03-27T...",
      "userName": "player1"
    },
    ...
  ]
}
```

**How to test:**
1. First, create some messages using "Save Chat Message" test
2. Click "Open" for this test (opens in new tab)
3. View the JSON response with all messages

---

### 2. Leaderboard Service Tests

#### **Test 3: Get Global Top Players**
- **URL:** `/test/leaderboard/global?count=10`
- **Method:** GET
- **Auth Required:** No
- **Parameters:** `count` (default: 10)

**What it does:**
- Retrieves top N users ordered by `TotalScore` descending
- Returns user details: Id, UserName, Email, TotalScore

**Expected Result:**
```json
{
  "success": true,
  "message": "Retrieved top 10 players",
  "data": [
    {
      "id": "user-1",
      "userName": "champion",
      "email": "champ@test.com",
      "totalScore": 5000
    },
    ...
  ]
}
```

**How to test:**
1. Click "Open" for this test
2. View the JSON response
3. If no users appear, register some users and give them scores by playing games

---

#### **Test 4: Get Game Results**
- **URL:** `/test/leaderboard/game?roomCode=XXXXX`
- **Method:** GET
- **Auth Required:** No
- **Parameters:** `roomCode` (required)

**What it does:**
- Retrieves detailed results for a completed game
- Includes:
  - Room code and category name
  - Final rankings (all players with scores and ranks)
  - Question-by-question breakdown (winner, points, correct answer)

**Expected Result:**
```json
{
  "success": true,
  "message": "Retrieved results for room ABC12",
  "data": {
    "roomCode": "ABC12",
    "categoryName": "Science",
    "finalRankings": [
      {
        "userName": "player1",
        "score": 300,
        "rank": 1
      },
      {
        "userName": "player2",
        "score": 150,
        "rank": 2
      }
    ],
    "questionResults": [
      {
        "order": 1,
        "questionText": "What planet is closest to the Sun?",
        "winnerName": "player1",
        "pointsAwarded": 100,
        "correctAnswer": "Mercury"
      },
      ...
    ]
  }
}
```

**How to test:**
1. Create a game and play it to completion (or manually set data in DB)
2. Note the room code from the game
3. Run: `/test/leaderboard/game?roomCode=YOUR_ROOM_CODE`
4. Verify all data is populated correctly

**Note:** This test is blocked until Gnsh's GameHub populates `GameQuestion.WinnerId` and `PointsAwarded`.

---

#### **Test 5: Get Current Standings**
- **URL:** `/test/leaderboard/standings?roomCode=XXXXX`
- **Method:** GET
- **Auth Required:** No
- **Parameters:** `roomCode` (required)

**What it does:**
- Retrieves current live standings for an in-progress game
- Returns list of players with current scores, ordered by score

**Expected Result:**
```json
{
  "success": true,
  "message": "Retrieved 3 players",
  "data": [
    {
      "userName": "player1",
      "score": 200,
      "rank": 1
    },
    {
      "userName": "player2",
      "score": 100,
      "rank": 2
    },
    {
      "userName": "player3",
      "score": 0,
      "rank": 3
    }
  ]
}
```

**How to test:**
1. Start a game but don't finish it
2. Run this endpoint with the room code
3. Verify scores match what's shown in the game UI

---

#### **Test 6: Finalize Game**
- **URL:** `/test/leaderboard/finalize/1`
- **Method:** POST
- **Auth Required:** Yes (must be logged in)
- **Parameters:** `gameId` in URL path

**What it does:**
- Updates each player's `User.TotalScore` by adding their game score
- This simulates what should happen when a game ends
- Calls `UserManager.UpdateAsync()` for each user

**Expected Result:**
```json
{
  "success": true,
  "message": "Game 1 finalized. Players' TotalScore updated.",
  "data": null
}
```

**How to test:**
1. Ensure game ID 1 has players with scores in `GamePlayers` table
2. Log in as any user
3. Click "Run" for this test
4. Check database: `User.TotalScore` should now include the game scores
5. Verify by running "Get Global Top" test

---

### 3. Data Setup Tests

#### **Test 7: Create Test Data**
- **URL:** `/test/create-data`
- **Method:** POST
- **Auth Required:** Yes (must be logged in)
- **Parameters:** None

**What it does:**
- Ensures at least one game exists (uses first available or creates one)
- Ensures at least one user exists
- Creates 5 test chat messages for game ID 1
- Adds test players with scores if none exist

**Expected Result:**
```json
{
  "success": true,
  "message": "Test data created for game ID 1",
  "data": {
    "gameId": 1,
    "roomCode": "ABC12",
    "messagesCreated": 5,
    "usersAvailable": 2
  }
}
```

**How to test:**
1. Use this first to set up data for other tests
2. Click "Run" while logged in
3. Then run chat and leaderboard tests

---

#### **Test 8: Clear Test Data**
- **URL:** `/test/clear-data?gameId=1`
- **Method:** POST
- **Auth Required:** Yes (must be logged in)
- **Parameters:** `gameId` (default: 1)

**What it does:**
- Deletes all chat messages for the specified game ID
- Useful for cleaning up between test runs

**Expected Result:**
```json
{
  "success": true,
  "message": "Cleared 5 chat messages for game ID 1",
  "data": null
}
```

**How to test:**
1. Run after creating test data
2. Verify by running "Get Chat History" - should return 0 messages

---

## Step-by-Step Testing Procedure

### **Full Test Sequence:**

1. **Setup:**
   - Register 2+ users via `/Account/Register`
   - Create a game via `/Lobby/Create`
   - Note the room code

2. **Initialize Test Data:**
   - Log in as one of the users
   - Navigate to `/test`
   - Click "Run" for **"Create Test Data"**
   - Verify success response

3. **Test Chat Service:**
   - Click "Run" for **"Chat Service - Save Message"**
   - Verify message saved successfully
   - Click "Open" for **"Chat Service - Get History"**
   - Verify the message appears in the list

4. **Test Leaderboard Service:**
   - Click "Open" for **"Leaderboard - Global Top 10"**
   - Should show at least one user (maybe no scores yet)
   - Click "Run" for **"Leaderboard - Finalize"** (POST)
   - Verify success response
   - Click "Open" for **"Leaderboard - Global Top 10"** again
   - Now users should have `TotalScore` values

5. **Test Game Results (When Available):**
   - After Gnsh implements GameHub and a game is completed:
   - Run **"Leaderboard - Game Results"** with the room code
   - Verify all question results and final rankings are populated

---

## Expected Outputs

### **Success Response Format:**
All endpoints return JSON with this structure:
```json
{
  "success": true,
  "message": "Human-readable success message",
  "data": { ... }  // or null
}
```

### **Error Response Format:**
```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

---

## Troubleshooting

### **Issue: "No users found"**
**Solution:** Register users first via `/Account/Register`

### **Issue: "No games found"**
**Solution:** Create a game via `/Lobby/Create` first

### **Issue: "Game with room code 'XXX' not found"**
**Solution:** Check that the room code exists in the database. Use the correct room code from a created game.

### **Issue: Chat messages not appearing**
**Solution:** 
- Ensure you ran "Create Test Data" or manually saved messages
- Check `gameId` parameter matches a game that exists

### **Issue: FinalizeGame doesn't update scores**
**Solution:**
- Ensure `GamePlayers` table has records with non-zero `Score` for the game
- Check that `User.Id` matches `GamePlayer.UserId`
- Verify `User.TotalScore` column exists and is an integer

### **Issue: GetGameResults returns empty QuestionResults**
**Solution:** This is expected until Gnsh's GameHub properly sets:
- `GameQuestion.WinnerId`
- `GameQuestion.PointsAwarded`
- `PlayerAnswer.IsWinningAnswer`

---

## Database Verification

If tests fail, check the database directly:

```sql
-- Check users
SELECT Id, UserName, TotalScore FROM AspNetUsers;

-- Check games
SELECT Id, RoomCode, Status FROM Games;

-- Check game players
SELECT GameId, UserId, Score FROM GamePlayers;

-- Check chat messages
SELECT Id, GameId, UserId, Text, SentAt FROM ChatMessages;

-- Check game questions
SELECT Id, GameId, QuestionId, Status, WinnerId, PointsAwarded FROM GameQuestions;
```

---

## Integration Checklist

Before handing off to Gnsh for GameHub integration:

- [ ] ChatService.SaveAsync works and persists messages
- [ ] ChatService.GetHistoryAsync retrieves messages with user names
- [ ] LeaderboardService.GetGlobalTopAsync returns users ordered by score
- [ ] LeaderboardService.FinalizeGameAsync updates User.TotalScore correctly
- [ ] LeaderboardService.GetGameResultsAsync returns complete data (once Gnsh populates it)
- [ ] LeaderboardService.GetCurrentStandingsAsync returns live scores (once scores update)
- [ ] All endpoints return proper JSON with success/error flags
- [ ] Error handling works (invalid gameId, missing roomCode, etc.)

---

## Notes

- **TestController is for development only** - Remove or disable before production
- All tests run against the real database - no mocking
- Some tests require authentication (SaveChatMessage, FinalizeGame, CreateData)
- Tests use default `gameId=1` - adjust if your game has different ID
- The "Create Test Data" endpoint helps set up initial state quickly
- Use the Test Dashboard UI for easy one-click testing
- API endpoints can also be called directly via browser or tools like Postman

---

## Quick Commands

```bash
# Build and run
cd QuizGame
dotnet run

# Access dashboard
http://localhost:XXXX/test

# Direct API calls
curl -X POST http://localhost:XXXX/test/chat/save?gameId=1&text=Hello
curl http://localhost:XXXX/test/chat/history?gameId=1
curl http://localhost:XXXX/test/leaderboard/global?count=10
curl -X POST http://localhost:XXXX/test/leaderboard/finalize/1
```

---

**Status:** All tests ready to run once database has users and games.  
**Next:** After verifying these services work, coordinate with Gnsh to integrate with GameHub.