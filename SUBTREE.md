# Git Subtree 동기화 가이드

이 폴더는 외부 저장소를 git subtree로 가져온 패키지입니다.

## 원본 저장소

- **Remote:** `weariness`
- **URL:** `https://github.com/weariness00/MyUnituPackage.git`
- **Branch:** `Util`

## Remote 등록 (최초 1회)

```bash
git remote add weariness https://github.com/weariness00/MyUnituPackage.git
```

## 최신 내용 가져오기 (Subtree Pull)

> 아래 명령은 **git 루트(프로젝트 최상위 폴더)** 에서 실행하세요.
> `find`로 `Weariness/Util` 폴더 위치를 자동 감지하므로 경로가 달라도 동작합니다.

```bash
git subtree pull --prefix="$(find . -type d -path "*/Weariness/Util" | sed 's|^\./||' | head -1)" weariness Util --squash
```

> **주의:** 작업 트리에 수정된 파일이 있으면 pull이 실패합니다.
> 먼저 `git stash`로 임시 저장 후 pull → `git stash pop` 순으로 진행하세요.

```bash
git stash
git subtree pull --prefix="$(find . -type d -path "*/Weariness/Util" | sed 's|^\./||' | head -1)" weariness Util --squash
git stash pop
```