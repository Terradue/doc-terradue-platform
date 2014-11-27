rm -rf packages/*
nuget restore
mkdir -p Terradue.Corporate.WebServer/core
mkdir -p Terradue.Corporate.WebServer/modules
cp -pr packages/**/content/core/** Terradue.Corporate.WebServer/core
cp -pr packages/**/content/modules/** Terradue.Corporate.WebServer/modules