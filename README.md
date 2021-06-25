# powerplant coding challenge

## Good Morning !

This is my participation to the powerplant coding challenge, sent to me by Extia Belgium.

## Description

This has been coded entirely in C#, with AspNetCore as framework, since it was preinstalled with visual studio, and there was no requirement for a specific framework.
Additionally, some libraries have been used for the development
- Newtonsoft.JSON
- Microsoft's test framework
- Swagger (for a nice docs design)

## How to run

The full project is included here, you can open the .sln and run it. It should work immediately upon clicking on the start button (probably named Powerplant).
It runs on the port 8888, defined in Program.cs

## How to deploy

Both normal deploy (inside a folder) and docker deploy are handled by visual studio directly, under the "Publish" option in the project's context menu.

## Running tests

The tests are in the "Tests" directory, except for the file TestUtils that only contains useful methods for testing, but not tests themselves.

--------------

# Project architecture

## Directories
- Controllers : Le projet API par défaut d'AspNetCore fonctionne sur un modèle MVC, ce dossier regroupe donc tous les controleurs du projet. Un seul actuellement, puisqu'il n'y a qu'un endpoint.

- Core : Contains most of the "active" code, not designed to discuss with the outer world.

- Tests : well. tests :)

## Files

### ProductionPlanController

The controller that receives the request from the user and returns the answer.
Nothing particularly interesting, a basic controller class with a logger.
It uses Fuels, Powerplant and Request files to parse the JSON query into a C# object, and passes it to the ProductionPlanComputer class

### ProductionPlanComputer

The class that manages most of the project. It contains the algorithm for the powerplant decision in the function named _Compute_.

There, there is multiple steps involved in the resolution of the problem.

- First, we try to solve the problem with only "renewable energies". In this example there's only one, but we can add more via the Powerplant.cs class. Solving with only renewable energies is very interesting because it costs nothing in this exercice.
    - In this case, the algorithm tries not to run one powerplant at full power, but to make them all run at slower paces (for example 50% of their pmax), to try to prevent noise as much as possible (for windmills for example)
- Then, if the renewable energies aren't enough, we add all the other powerplants, after ordering them based on their cost (efficiency in code). During this phase, we try to limit the amount of running powerplants, so we run them as much as we can (pmax)
- If we need some additional energy but the next powerplant's pmin is too high (eg. we need 50MWH and the pmin is 100MWH) we try (with function FallbackReducePower) to limit the previous powerplants so the last one produces (with its pmin) the exact value we need to reach. The least efficient powerplants have their power lowered first obviously
- Finally (i'm not certain that could happen in this example, but that will definitely happen in a real-life scenario), if degrading the performance of some of the previous powerplants (or adding some running at half power for example) makes the production not sufficient to match the request, a last pass (FallbackIncreasePower) will increase again (starting from the most efficient) the power output of the powerplants.

## Additional tasks

- The algorithm takes the CO2 price into account.
    - Configuration of the CO2 generation in Config class
    - Added to the efficiency of each powerplant in PowerplantEfficiency.GetEfficiency, based on whether the powerplant uses green energy or not.
- Docker image can be immediately generated through visual studio's publish function

## Personal note

This was the most interesting coding challenge i ever had as part of a hiring process. Thank you for not doing another fizzbuzz.

## Thanks !