COLOR='\033[0;36m'
NC='\033[0m' # No Color

echo " "

echo "${COLOR}Pushing build for OSX${NC}"
butler push Builds/osx anttihaavikko/just-fishin:osx --fix-permissions

echo "${COLOR}Pushing build for Windows${NC}"
butler push Builds/win anttihaavikko/just-fishin:win --fix-permissions

echo "${COLOR}Pushing build for Linux${NC}"
butler push Builds/linux anttihaavikko/just-fishin:linux

echo "${COLOR}Copying html5 files over to correct path"
cp -a Builds/webgl/html5/Build/. Builds/html5/Build
echo "${COLOR}Pushing build for HTML5${NC}"
butler push Builds/html5 anttihaavikko/just-fishin:html5
