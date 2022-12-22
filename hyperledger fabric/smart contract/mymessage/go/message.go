/*
 * SPDX-License-Identifier: Apache-2.0
 */

package main

// Imessage stores a value
type Message struct {
	Data      string `json:"data"`
	Signature string `json:"signature"`
	Counter   int32  `json:"counter"`
}
