# cs-server

C# 서버 프로젝트입니다.
프로젝트 개요 및 코드설계에 관한 내용은 Docs에 자세하게 정리되어 있습니다.


서버 간단소개
1. C#을 활용한 Room 기반 채팅서버 입니다.
2. TCP / 스레드풀 기반 멀티스레드를 사용하였습니다.
3. 코드의 장기적인 유지/보수/확장성을 위해 OOP패턴을 다수 사용하였습니다.


서버 시작

1. 서버 빌드 : dotnet build
2. 서버 시작 : dotnet run
3. 채팅 참여 : nc 127.0.0.1 7777
4. 서버 종료 : ctrl + c


개발자 호스트 서버 스팩

- Hardware - Apple Mac M2 Air
- .NET Version - 9.0.300
- C# Version - VS Code Extension C# v2.50.27(Microsoft)


서버 패키지 참고

- 본 서버는 아래의 패키지를 추가하여 사용합니다.
- 터미널에에서 아래 코드를 입력하시면 빌드에 필요한 패키지가 자동 추가됩니다.

dotnet add package Serilog
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Sinks.Console
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json




License
- MIT 라이선스로 배포합니다.
