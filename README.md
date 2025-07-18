# 🛡️ **Unity Wii Play Tanks (Full Fan Re‑Creation)**

A faithful, **100 % feature‑complete** recreation of the iconic **Tanks!** minigame from *Wii Play*—rebuilt from the ground up in **Unity**.  
Every level, every shell ricochet, every clanking tread, and even the mischievous AI surprises are here, with modern twists like ML‑driven opponents and game‑pad remapping.

<p align="center">
  <img src="docs/hero.gif" alt="Gameplay montage" width="700">
</p>

[![Unity Version](https://img.shields.io/badge/Unity-2022.3%20LTS-blue?logo=unity)](#prerequisites) 
![GitHub license](https://img.shields.io/github/license/YOUR‑ORG/wii‑tanks‑unity) 

---

## ✨ Features

| Core                               | Modern Extras                         |
|------------------------------------|---------------------------------------|
| 100 Wii‑Play levels, plus 10 bonus | **🎮 Controller support** (Xbox, DS4, Switch Pro, etc.) |
| Realistic ricochet & terrain destruction | **🧠 Two AI pipelines**<br>• *Algorithmic* (classic path‑finding & state machines)<br>• *ML‑Agents* reinforcement bots |
| Split‑screen co‑op & versus        | **⚡ Dynamic VFX** (GPU instancing, URP decals) |
| Fully remixed soundtrack & SFX     | **📈 Mod‑ready architecture**—ScriptableObjects + Addressables |
| In‑game level selector & stats     | **🖼️ Hand‑painted + AI‑generated concept art** |

---

## 📦 Project Structure
```text
/Assets
  /Art                # Stylized low‑poly assets + AI up‑scaled textures
  /Scripts
    /Gameplay         # Tank, shell, explosion logic
    /AI
      /Classic        # A*, state machines
      /MLAgents       # PPO‑trained brains & trainers
  /Scenes
  /Tests
```

---

## 🚀 Getting Started

### Prerequisites
* **Unity 2022.3 LTS** (URP template)  
* .NET 6 SDK (for CLI tooling & tests)  
* Git LFS (large binary assets)

### 1 · Clone
```bash
git clone --recursive https://github.com/YOUR-ORG/wii-tanks-unity.git
```

### 2 · Open  
Open the project in **Unity Hub**, selecting the *URP* renderer when prompted.

### 3 · Play / Build
* Press **Play** to test in the editor.  
* Use **Build & Run** for Windows, macOS, or WebGL.

---

## 🎮 Controls

| Action               | Keyboard (default) | Game‑Pad (Xbox layout) |
|----------------------|--------------------|------------------------|
| Move tank            | **W A S D**        | Left Stick             |
| Aim turret           | Mouse             | Right Stick            |
| Fire / Confirm       | Left Click        | **RT**                 |
| Drop mine            | Space             | **LT**                 |
| Pause / Menu         | Esc               | **Start**              |

> **Tip:** All bindings can be remapped under **Settings → Controls**.

---

## 🤖 AI Details

* **Classic Mode** — deterministic AI using A* for navigation, predictive ricochet targeting, and finite‑state combat logic.  
* **ML Mode** — Unity ML‑Agents (PPO) trained on reward functions for survival, multi‑kill chains, and ricochet mastery. Swap brains in **AIManager** at runtime.

Training configs live in `/ML/trainers/`. Re‑train with:
```bash
mlagents-learn config/ppo_tank.yaml --run-id=tanks_v1
```

---

## 🖼️ Art Pipeline

| Source            | Toolchain                                    |
|-------------------|----------------------------------------------|
| Concept sketches  | Procreate ➜ Midjourney v6 (prompt‑guided)    |
| Low‑poly models   | Blender 4.0 + Hard‑Ops                       |
| Textures          | Substance Painter ➜ Topaz Gigapixel AI       |

---

## 📜 Contributing

1. **Fork** → create a branch (`feat/your-feature`)  
2. Ensure **Unity Tests** pass (`npm run test`)  
3. Open a **PR** with clear description & screenshot/GIF  
4. One of us will review & merge 🚀

---

## 🗺️ Roadmap

- [ ] Steam Remote Play Together integration  
- [ ] Online co‑op via **Netcode for GameObjects**  
- [ ] Global leaderboards (PlayFab)  
- [ ] Experimental VR mini‑mode  

*Open an issue* to suggest your own ideas!

---

## 📚 Acknowledgements

* **Nintendo EAD** — original *Wii Play* (2006) inspiration  
* **Unity Technologies** — Engine & ML‑Agents  
* **OpenAI / Midjourney** — AI art generation  
* Community beta testers ♥  

> **Disclaimer:** This is a **non‑commercial fan project** intended for educational and nostalgic purposes. All original *Wii Play* assets and trademarks are property of Nintendo. If you are a rights holder and have concerns, please open an issue and we will respond immediately.

---

## 🛡️ License

Distributed under the **MIT License**.  
See [`LICENSE`](LICENSE) for details.

---

<p align="center">
  <img src="docs/thanks.gif" alt="Thanks for playing!" width="500">
</p>
