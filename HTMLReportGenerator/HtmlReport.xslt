<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:lb="http://library.by/catalog"
                extension-element-prefixes="lb">
    <xsl:output method="html" encoding="utf-8" indent="yes" />

    <xsl:param name="Date" select="''"/>

    <xsl:template match="/">
        <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html&gt;</xsl:text>
        <html>
            <head>
                <title>Текущие фонды по жанрам</title>
                <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.1.3/css/bootstrap.min.css" integrity="sha384-MCw98/SFnGE8fJT3GXwEOngsV7Zt27NXFoaoApmYm81iuXoPkFOJwJ8ERdknLPMO" crossorigin="anonymous"/>
            </head>
            <body>
                <div class="container">
                    <h1>Текущие фонды по жанрам <xsl:value-of  select="$Date"/></h1>
                    <xsl:apply-templates select="@* | node()"/>
                    Всего книг : <xsl:value-of select="count(//lb:book)"/>
                </div>
            </body>
        </html>
    </xsl:template>

    <xsl:template match="lb:catalog">

        <xsl:call-template name="bookReport">
            <xsl:with-param name="GenreName">Computer</xsl:with-param>
        </xsl:call-template>

        <xsl:call-template name="bookReport">
            <xsl:with-param name="GenreName">Fantasy</xsl:with-param>
        </xsl:call-template>

        <xsl:call-template name="bookReport">
            <xsl:with-param name="GenreName">Romance</xsl:with-param>
        </xsl:call-template>

        <xsl:call-template name="bookReport">
            <xsl:with-param name="GenreName">Horror</xsl:with-param>
        </xsl:call-template>

        <xsl:call-template name="bookReport">
            <xsl:with-param name="GenreName">Science Fiction</xsl:with-param>
        </xsl:call-template>

    </xsl:template>

    <xsl:template name="bookReport">
        <xsl:param name="GenreName">Empty</xsl:param> 
        <h2>
            <xsl:value-of select="$GenreName"/>
        </h2>
        <table class="table">
            <tr>
                <th scope="col">Автор</th>
                <th scope="col">Название</th>
                <th scope="col">Дата издания</th>
                <th scope="col">Дата регистрации</th>
            </tr>
            <xsl:for-each select="lb:book/lb:genre[text()=$GenreName]">
                <tr>
                    <td>
                        <xsl:value-of select="parent::node()/lb:author"/>
                    </td>
                    <td>
                        <xsl:value-of select="parent::node()/lb:title"/>
                    </td>
                    <td>
                        <xsl:value-of select="parent::node()/lb:publish_date"/>
                    </td>
                    <td>
                        <xsl:value-of select="parent::node()/lb:registration_date"/>
                    </td>
                </tr>
            </xsl:for-each>
            <tr>
                <td colspan="4">
                    Всего книг жанра <xsl:value-of select="$GenreName"/> : <xsl:value-of select="count(lb:book/lb:genre[text()=$GenreName])"/>
                </td>
            </tr>
        </table>
    </xsl:template>

</xsl:stylesheet>