## Headers

# Heading 1 (h1)
## Heading 2 (h2)
### Heading 3 (h3)
#### Heading 4 (h4)
##### Heading 5 (h5)
###### Heading 6 (h6)

------------------------------------------

## Horizontal Rules

Underscores _

___

Dashes -

---

Stars *

***

------------------------------------------

## Text Formatting / Emphasis

*This is italic text* (single */_)

**This is bold text** (double */_)

__*Bold and italic combined*__ (_/* combined)

~~Strikethrough~~ (double ~)

------------------------------------------

## Lists

### Unordered

* Create a list by starting a line with `+`, `-`, or `*`
* Sub-lists are made by indenting 2 spaces:
  * Marker character change forces new list start:
    * Item 1
    + Item 2
    - Item 3
* Very easy!

### Ordered

1. Create a list by starting a line with `1.`
2. Sub-lists are made by indenting 3 spaces:
   1. The depth of the list determines the list style:
      1. Item 1
      1. Item 2
      1. Item 3
3. Very easy!

------------------------------------------

## Blockquotes

> Blockquotes can also be nested...
>> ...by using additional greater-than signs right next to each other...
> > > ...or with spaces between arrows.

------------------------------------------

## Tables

| Option | Description |
| ------ | ----------- |
| data   | path to data files to supply the data that will be passed into templates. |
| engine | engine to be used for processing templates. Handlebars is the default. |
| ext    | extension to be used for dest files. |

Right aligned columns

| Option | Description |
| ------:| -----------:|
| data   | path to data files to supply the data that will be passed into templates. |
| engine | engine to be used for processing templates. Handlebars is the default. |
| ext    | extension to be used for dest files. |

------------------------------------------

## Code

Inline `code`

Indented code

    // Some comments
    line 1 of code
    line 2 of code
    line 3 of code


Block code "fences"

```
Sample text here...
```

Syntax highlighting

``` js
var foo = function (bar) {
  return bar++;
};

console.log(foo(5));
```