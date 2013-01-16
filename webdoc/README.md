Webdoc
======

Webdoc is the web container for monodoc. It normally includes:

 - a header
 - a footer
 - a navigation tree
 - the monodoc iframe

Structure
---------

Each webdoc instance consists of a skin (theme, chrome) and several plugins. 

Plugins are located in the plugins directory. The plugins currently available are:

 - iframe: helps size the iframe correctly in your webdoc instance. Recommended unless you want to 
use your own code for that
 - sidebar: left navigation tree. Again, recommended unless you feel like writign and wiring up your own (good luck!)
 - fast search: searches while you type
 - full search: returns a page of search results (`search.html`). Can be styled however you like.

Skins are located in the skins directory. Each skin consists of:

 - `header.html` (required)
 - `footer.html` (required)
 - additional css and js (usually placed in the `common-extension.css`/`common-extension.js` file)
 - images folder (optional)

How to Use
----------

Making a new instance of webdoc is easy. First, you need to edit web.config to point 
to the location of your monodoc source root.

`MonodocRootDir` -> monodoc source root

Next, throw your skin into the skins directory (or use one that's already there).

Edit the `plugins.def` file, uncommenting the plugins you want to use, and setting the location of your skin.
