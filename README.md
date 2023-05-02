# DtsHelper #

Simple command line based helper application for SSIS and DTS code injection.

## Description ##

This application was developed for providing a mechanismn for handling sql files directly in SSIS dtsx packages. Normally sql files are not directly supported. Instead only FileConnections exists which require to share sql files on directory. This makes deployment and versioning hard especially with multiple environments like development, staging and production.

To fill this gap this application supports code injection of sql files directly in dtsx packages. Therefore SSIS deployment package contains all sql content which is needed for execution and no other external dependency exists. 