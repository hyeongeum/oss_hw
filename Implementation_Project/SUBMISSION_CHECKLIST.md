# Project A+ 제출 체크리스트

## 제출할 항목

- 실행본 제출: `outputs/ProjectAPlus_Submission.zip`
- Unity 원본 프로젝트 제출: `outputs/ProjectAPlus_UnityProject_Submission.zip`

실행본 ZIP에는 Windows 빌드 폴더와 문서가 포함되어 있습니다. Unity 원본 프로젝트 ZIP에는 `Assets`, `Packages`, `ProjectSettings`와 문서가 포함되어 있으며 `Library`, 로그, 임시 파일은 제외되어 있습니다.

## 실행 방법

1. `outputs/ProjectAPlus_Windows/Project A+.exe` 실행
2. 또는 저장소 루트의 `PLAY_PROJECT_A_PLUS.bat` 실행

`Project A+.exe`만 단독으로 옮기면 실행되지 않습니다. `Project A+_Data`, `MonoBleedingEdge`, `UnityPlayer.dll`을 포함한 빌드 폴더 전체가 필요합니다.

## 최종 검증 결과

- Unity `2022.3.62f3` 컴파일 오류 및 경고 없음
- 오프라인 실행 및 네트워크 API 미사용 확인
- 제공 이미지 기반 런타임 에셋 49개 임포트 검사 통과
- Stage 1~10 생성, 전투, 지형 도달성 검사 통과
- 엘리트 몬스터 1.5배 크기 및 피격 판정 검사 통과
- Stage 5·10 보스 카메라 스크롤 검사 통과
- 타이틀 → 오프닝 → Stage 1~10 → 엔딩 → 최종 성적 흐름 통과
- 저장, 이어하기, 재도전 체크포인트, 일시정지 복귀 검사 통과
- 1280x720 및 1920x1080 Windows 실행 검사 통과
