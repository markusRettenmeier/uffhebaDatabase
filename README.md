# README #

This software is designed to create a database for collecting and sharing collectibels. The database is designed to be flexible and can be used for various types of collections, such as coins, stamps, or any other type of collectible items. The software allows users to create their own collections, add items to their collections, and share their collections with others. The software is open source and can be used by anyone who wants to contribute to the project.
Right now, all features and AI-integrations are 100% free for the community to use and develop. As there are more users interested in using this platform, heavy usage of the servers, advanced modules are required and additional features are added, it becomes increasingly expensive to maintain. So later the project must be financed either by donations, "Bring your own key" (BYOK), adds or by a subscription model, depending on what fits and is accepted. We promise to always keep the core open-source engine free!

### What is this repository for? ###

In this open repository you find the code to the database of uffheba. If you have any interest to support this platform and add / change code feel free to do so.
* Version 1.0.0
* [Learn Markdown](https://bitbucket.org/tutorials/markdowndemo)

### How do I get set up? ###
To get started with this project, you can follow these steps:
1. Clone the repository to your local machine using the command: `git clone https://github.com/uffheba/uffhebaDatabase.git`
2. Navigate to the project directory: `cd uffheba-database`
3. Install the required dependencies using the command: `pip install -r requirements.txt`
4. Add appsettings.json with the following content:
```json
{
  "ConnectionStrings": {
    "DbIdentityContextConnection": "Server=...;Database=...;MultipleActiveResultSets=true;User Id=...;Password=...;TrustServerCertificate=True"
  },
  "SendGridKey": "...",
  "DeepL": {
    "ApiKey": "..."
  }
}
```

### Contribution guidelines ###

There is additionally a program, where the test is available
* Code review
* Other guidelines

### Who do I talk to? ###

uffheba with address: Markus Rettenmeier, Gotthilf-Dorn-Str. 25, 89143 Blaubeuren, Germany is the owner of this repo
Conteact me/us under markus.rettenmeier@web.de

### License ###
This project is licensed under the AGPLv3 License - see the [LICENSE](LICENSE) file for details

### Funding ###
If you want to contibute a coffee: use github sponsoFrs: https://github.com/sponsors/markusRettenmeier