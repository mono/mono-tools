Configuration options
=====================

Webdoc can be configured with the following variables in the web.config file:

 - `MonodocRootDir`: point where you monodoc source root is
 - `GoogleAnalytics`: if you want to register your webdoc instance for analytics, enter your API key in that variable
 - `ExternalHeader`/`ExternalFooter`: path to an external asset definition file for custom header/footer, definition of the file given below

External asset file syntax
--------------------------

Three types of ressources can be declared: `html`, `css` and `javascript`. You usually wants at least `html` but none of the fields are mandatory. You declare one resource per line in a key-value pair separated by an equal ('=') sign with the right hand-side being the path to the asset.

An example of such file follows:

      # Comments begin with a hash
      html=external/header.html
      css=external/header.css
      javascript=external/header.js

