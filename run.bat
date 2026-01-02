@echo off
set IMAGE_NAME=tungb12ok/que_exe
set DOCKERFILE_PATH=Web\Dockerfile
set CONTEXT=.

echo ----------------------------
echo Building Docker image...
echo ----------------------------
docker build -t %IMAGE_NAME% -f %DOCKERFILE_PATH% %CONTEXT%
IF %ERRORLEVEL% NEQ 0 (
    echo ❌ Build failed!
    pause
    exit /b %ERRORLEVEL%
)

echo ----------------------------
echo Pushing Docker image...
echo ----------------------------
docker push %IMAGE_NAME%
IF %ERRORLEVEL% NEQ 0 (
    echo ❌ Push failed!
    pause
    exit /b %ERRORLEVEL%
)

echo ✅ DONE: Image %IMAGE_NAME% has been pushed successfully.
pause
