# Project Roadmap - 8 Week Learning Plan

## Current Status

### ✅ Completed (Week 1-2)

**Week 1:**
- JWT Bearer authentication system (5 endpoints)
- Character growth system (6 endpoints)
- Auto-stat progression (Level → Stats)
- Monster entities (5 types seeded)
- PostgreSQL + EF Core migrations
- Jenkins CI/CD pipeline
- AWS EC2 + RDS deployment
- Unit tests (17 tests passing)

**Week 2:**
- Combat system core logic
- Offline reward calculation
- Background service (IHostedService)
- Battle log system

**Week 3 Day 1:**
- Equipment System (7 endpoints)
- Inventory management (OwnerId vs CharacterId pattern)
- Equipment enhancement foundation

### 🔄 Current Focus (Week 3)

- Combat System integration with Equipment stats
- Dungeon progression system
- Equipment enhancement (probability-based)

### 📋 Next Priority (Week 3-6)

- Skill system foundation
- Pet system basics

---

## 8-Week T-Shaped Learning Strategy

**Core Philosophy**:
- **6 core systems** at **95% depth** (Vertical - Master level)
- **14 supporting systems** at **60-80% breadth** (Horizontal - Understanding level)

**See**: Serena memory `development_roadmap_checklist` for detailed task breakdown

---

## Phase 1: Core Vertical Slice (Week 1-3)

**6 Systems at 95% Depth**

1. ✅ **Authentication System (JWT)** - COMPLETED
2. ✅ **Character Growth System** - COMPLETED
3. ⏳ **Combat System** (Auto-battle) - IN PROGRESS
4. ⏳ **Inventory & Equipment** - IN PROGRESS
5. ⏳ **Offline Rewards** - COMPLETED (basic)
6. ⏳ **Dungeon System** - PENDING

### Learning Focus
- Complete game loop: Login → Battle → Progression
- Clean Architecture mastery
- Serilog from day 1
- RESTful API best practices
- EF Core advanced patterns
- IHostedService background jobs

---

## Phase 2: Core Features + SignalR (Week 4-5)

**4 Systems with SignalR at 90% Depth**

7. 📋 **Equipment Enhancement**
8. 📋 **Skill System**
9. 📋 **Pet System**
10. 📋 **Real-time Chat (SignalR)** ← **Deep Learning**

### Learning Focus
- **SignalR real-time communication mastery (90% depth)**
- Unity ↔ Server integration
- CQRS with MediatR
- FluentValidation
- AutoMapper patterns

---

## Phase 3: Social + Redis (Week 6-7)

**4 Systems with Redis at 90% Depth**

11. 📋 **Friend System**
12. 📋 **Guild System** (Basic + Raid)
13. 📋 **Ranking System (Redis)** ← **Deep Learning**
14. 📋 **Boss Raid** (Cooperative)

### Learning Focus
- **Redis mastery (90% depth)**
  - Sorted Set for ranking
  - Distributed locking
  - Multi-instance coordination
- Guild collaboration mechanics
- Boss raid scaling

---

## Phase 4: Monetization (Week 8)

**6 Systems at 50-70% Breadth**

15. 📋 **Quest & Achievement** (70%)
16. 📋 **Daily Mission & Attendance** (70%)
17. 📋 **Gacha System** (70% - probability logic)
18. 📋 **Shop & VIP** (60%)
19. 📋 **Mail System** (60%)
20. 📋 **Event System** (50%)

### Learning Focus
- Rapid implementation for broad coverage
- Concept understanding over perfect implementation
- Gacha probability mathematics
- Event scheduling patterns
- Monetization system patterns

---

## Learning Milestones by Week

| Week | Technologies | Depth | Key Deliverables |
|------|-------------|-------|------------------|
| **1-3** | RESTful API, EF Core, JWT, IHostedService, Serilog, Clean Architecture | 95% | Complete game loop (6 systems) |
| **4-5** | SignalR, MediatR CQRS, FluentValidation, AutoMapper | 90% | Real-time features (4 systems) |
| **6-7** | Redis (ranking, locking), Guild patterns, Raid mechanics | 90% | Social features (4 systems) |
| **8** | Gacha probability, Event scheduling, Shop patterns | 50-70% | Monetization (6 systems) |

---

## Scope Management

### ✅ In Scope (20 Systems)

All 20 systems listed above with varying depth levels

### ❌ Out of Scope

- **IAP (In-App Purchase)**: Platform integration too time-consuming
- **FCM (Push Notification)**: Platform setup overhead
- **Firebase Analytics**: Non-core learning objective
- **Application Insights**: Concept understanding only, no implementation

### ⚠️ Deferred to Post-8-Week

- **PVP Arena**: ELO rating system complexity requires more time

---

## PRD References

- **Week 2 PRD**: `.taskmaster/docs/week2-prd.txt` - Idle Game Loop (Combat, Offline Rewards)
- **Complete PRD**: `docs/MUSHROOM_GAME_PRD.md` - Original 20-system specification
- **Tech Stack Priorities**: Serena memory `tech_stack`
- **Detailed Task Breakdown**: Serena memory `development_roadmap_checklist`

---

## Progress Tracking

**How to Update This Document:**

1. Move completed systems from "In Progress" to "Completed"
2. Update current week focus
3. Add new checkpoints from Serena memory
4. Keep Serena memory `development_roadmap_checklist` as source of truth

**Checkpoint Frequency:**
- Daily: Update current focus
- Weekly: Update phase completion status
- Milestones: Create Serena checkpoint memory

---

**Last Updated**: 2025-10-17 (Week 3 Day 1)
