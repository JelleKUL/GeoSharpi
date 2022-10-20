# DocFX Documentation Guide

This Article will explain how to generate API documentation for your project and how to publish it online using Github Actions.

## Folder Structure

```
Documentation~/         # The root of the Documentation, the "~" Hides it from Unity
├─ DocFx/               # The main folder of the DocFx Files
│  ├─ docfx.json        # The docfx configuration file
│  ├─ toc.yml           # The main table of contents
│  ├─ filterconfig.yml  # The API namespace filter
│  ├─ articles/         # Folder of the hand written articles
│  │  ├─ toc.yml        # The table of contents for the articles
│  │  ├─ "article1".md  
│  │  ├─ "article2".md  
│  │
│  ├─ resources/        # The folder for website files
│  │  ├─ logo.svg       
│  │  ├─ favicon.ico  
│  │  
│  ├─ images/           # The folder for images in the articles
│  │  ├─ "image1"       
│  │  ├─ "image2"       
│  │  
│  
README.md               # The main README file in the root of the project

```

## docfx.json

The main configuration file to generate the APi documentation.
All the files need to be added to `content` 