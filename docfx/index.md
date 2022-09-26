[!include[readme](../README.md)]

## Docs

The API docs are currently built with docfx using xml comments in the code. 

There's a docfx.json file in this directory. To build these docs you need to:

1. Get the latest docfx - 2.59.4.0+ (if something goes wrong - start with that version)
1. [optional] put `docfx` on your path
1. `cd` to this directory
1. Remove the existing `${CHECKOUT_ROOT}/docs` folder (`../docs` from this folder) and `./metadata` folder
1. Run `docfx metadata` to regenerate the metadata 
1. Run `docfx build` to regenerate the site (or `docfx build --serve` to build and preview in one step)
1. Run `docfx serve ../docs` to preview the content
1. When happy, commit the `docs` and any changes. The site will update once pushed to main
