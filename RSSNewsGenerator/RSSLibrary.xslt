<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" 
                xmlns:lb="http://library.by/catalog"
                exclude-result-prefixes="msxsl"
                extension-element-prefixes="lb">
    <xsl:output method="xml" indent="yes"/>

    <xsl:template match="/lb:catalog">
        <rss version="2.0" xmlns:content="http://purl.org/rss/1.0/modules/content/">
            <channel>
                <title>Ночная библиотека им. Альбатроса Газгольдеровича Двутаврова</title>
                <link>http://library.by/catalog</link>
            </channel>
            <xsl:apply-templates select="@* | node()"/>
        </rss>
    </xsl:template>

    <xsl:template match="lb:book">
        <item>
            <title>
                <xsl:value-of select="lb:title"/>
            </title>
            <author>
                <xsl:value-of select="lb:author"/>
            </author>
            <pubDate>
                <xsl:value-of select="lb:registration_date"/>
            </pubDate>
            <xsl:if test="(lb:isbn) and (lb:genre/text() = 'Computer')">
                <link>http://my.safaribooksonline.com/<xsl:value-of select="lb:isbn"/></link>
            </xsl:if>
        </item>
    </xsl:template>

    <xsl:template match="@* | node()">
            <xsl:apply-templates select="@* | node()"/>
    </xsl:template>

</xsl:stylesheet>
 